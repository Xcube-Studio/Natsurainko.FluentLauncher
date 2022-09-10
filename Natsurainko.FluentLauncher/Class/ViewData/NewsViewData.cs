using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class NewsViewData : ViewDataBase<NewsModel>
{
    public NewsViewData(NewsModel data) : base(data)
    {
        _ = BeginDownloadImageAsync();
    }

    [Reactive]
    public BitmapImage ImageSource { get; set; }

    [Reactive]
    public Visibility Loading { get; set; } = Visibility.Visible;

    public string Description => $"{this.Data.Tag}{(string.IsNullOrEmpty(this.Data.Tag) ? string.Empty : " ")}{ConfigurationManager.AppSettings.CurrentLanguage.GetString("ANP_Description")}";

    public async Task BeginDownloadImageAsync()
    {
        var res = await HttpWrapper.HttpGetAsync($"https://launchercontent.mojang.com/{this.Data.NewsImage.Url}");

        DispatcherHelper.RunAsync(async delegate
        {
            using var stream = await res.Content.ReadAsStreamAsync();

            ImageSource = new BitmapImage();
            await ImageSource.SetSourceAsync(stream.AsRandomAccessStream());
            Loading = Visibility.Collapsed;

            res.Dispose();
        });
    }
}

public class NewsModel
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("tag")]
    public string Tag { get; set; }

    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("newsPageImage")]
    public NewsPageImage NewsImage { get; set; }

    [JsonProperty("readMoreLink")]
    public string Link { get; set; }

    public class NewsPageImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}


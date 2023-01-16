using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Models;

public partial class NewsData : ObservableObject
{
    [ObservableProperty]
    private JObject newsPageImage;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string tag;

    [ObservableProperty]
    private string date;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private string readMoreLink;

    [ObservableProperty]
    private BitmapImage imageSource;

    [ObservableProperty]
    private Visibility loading = Visibility.Visible;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(NewsPageImage))
            Load();
    }

    public void Load()
    {
        Task.Run(async () =>
        {
            if (imageSource == null)
            {
                var res = await HttpWrapper.HttpGetAsync($"https://launchercontent.mojang.com/{newsPageImage["url"]}");

                App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    using var stream = await res.Content.ReadAsStreamAsync();

                    ImageSource = new BitmapImage();
                    await ImageSource.SetSourceAsync(stream.AsRandomAccessStream());
                    Loading = Visibility.Collapsed;

                    res.Dispose();
                });
            }
            else Loading = Visibility.Collapsed;
        });

    }
}

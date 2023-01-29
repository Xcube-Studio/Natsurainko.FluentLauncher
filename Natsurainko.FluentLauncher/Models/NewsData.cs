using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Models;

public partial class NewsData : ObservableObject
{
    public class NewsImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        public string Uri => $"https://launchercontent.mojang.com{Url}";
    }

    [ObservableProperty]
    private NewsImage newsPageImage;

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
}

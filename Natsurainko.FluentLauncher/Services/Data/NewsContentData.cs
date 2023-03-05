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

namespace Natsurainko.FluentLauncher.Services.Data;

internal class NewsContentData
{
    public NewsContentData(string imageUrl, string title, string tag, string date, string text, string readMoreUrl)
    {
        ImageUrl = imageUrl;
        Title = title;
        Tag = tag;
        Date = date;
        Text = text;
        ReadMoreUrl = readMoreUrl;
    }

    public string ImageUrl { get; }

    public string Title { get; }

    public string Tag { get; }

    public string Date { get; }

    public string Text { get; }

    public string ReadMoreUrl { get; }
}

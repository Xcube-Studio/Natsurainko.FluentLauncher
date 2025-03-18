using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Views.News;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.News;

internal partial class NoteViewModel(CacheInterfaceService cacheInterfaceService) 
    : PageVM<NotePage>, INavigationAware
{
    [ObservableProperty]
    public partial string Body { get; set; }

    [ObservableProperty]
    public partial PatchNoteData PatchNoteData { get; set; }

    void INavigationAware.OnNavigatedTo(object? parameter) => PatchNoteData = (parameter as PatchNoteData)!;

    public override void OnLoaded()
    {
        cacheInterfaceService.RequestStringAsync(
            $"https://launchercontent.mojang.com/v2/{PatchNoteData.ContentPath}",
            Services.Network.Data.InterfaceRequestMethod.Static)
        .ContinueWith(task =>
        {
            string patchJson = task.Result!;
            string body = JsonNode.Parse(patchJson)!["body"]!.GetValue<string>();
            body = "<style>img{width:auto;height:auto;max-width:100%;max-height:100%;}</style>" + body;

            Dispatcher.TryEnqueue(async () =>
            {
                var WebView2 = this.Page.WebView2;

                await WebView2.EnsureCoreWebView2Async();

                WebView2.CoreWebView2.Profile.PreferredColorScheme = WebView2.ActualTheme == ElementTheme.Dark
                    ? CoreWebView2PreferredColorScheme.Dark
                    : CoreWebView2PreferredColorScheme.Light;

                body = $"<meta name=\"color-scheme\" content=\"{(WebView2.ActualTheme == ElementTheme.Dark ? "dark light" : "light dark")}\">\r\n" + body;

                WebView2.NavigateToString(body);
            });

        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }
}

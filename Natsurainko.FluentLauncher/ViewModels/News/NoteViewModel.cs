using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.News;

internal partial class NoteViewModel : ObservableObject, INavigationAware
{
    private readonly CacheInterfaceService _cacheInterfaceService;


    public NoteViewModel(CacheInterfaceService cacheInterfaceService)
    {
        _cacheInterfaceService = cacheInterfaceService;
    }

    [ObservableProperty]
    private string body;

    [ObservableProperty]
    private PatchNoteData patchNoteData;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        PatchNoteData = parameter as PatchNoteData;
    }

    [RelayCommand]
    public void LoadedEvent(object args)
    {
        var sender = args.As<WebView2, object>().sender;

        void ParseBodyTask(Task<string> task)
        {
            if (task.IsFaulted)
            {
                return;
            }

            string patchJson = task.Result;
            string body = JsonNode.Parse(patchJson)!["body"].GetValue<string>();
            body = "<style>img{width:auto;height:auto;max-width:100%;max-height:100%;}</style>" + body;

            App.DispatcherQueue.TryEnqueue(async () =>
            {
                await sender.EnsureCoreWebView2Async();

                if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    sender.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
                    body = "<meta name=\"color-scheme\" content=\"dark light\">\r\n" + body;
                }

                sender.NavigateToString(body);
            });
        }

        _cacheInterfaceService.RequestStringAsync(
            $"https://launchercontent.mojang.com/v2/{PatchNoteData.ContentPath}",
            Services.Network.Data.InterfaceRequestMethod.Static)
        .ContinueWith(ParseBodyTask);
    }
}

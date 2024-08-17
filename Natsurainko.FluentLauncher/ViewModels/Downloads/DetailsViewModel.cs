﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DetailsViewModel : ObservableObject, INavigationAware
{
    private object _resource;

    private readonly INavigationService _navigationService;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public DetailsViewModel(
        INavigationService navigationService, 
        CurseForgeClient curseForgeClient,
        ModrinthClient modrinthClient)
    {
        _navigationService = navigationService;
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
    }

    [ObservableProperty]
    private string iconUrl;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string summary;

    [ObservableProperty]
    private string webLink;

    [ObservableProperty]
    private string authors;

    [ObservableProperty]
    private int downloadCount;

    [ObservableProperty]
    private DateTime dateModified;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasScreenshot))]
    private string[] screenshotUrls;

    [ObservableProperty]
    private bool isHtmlDescription = false;

    public bool HasScreenshot => ScreenshotUrls != null && ScreenshotUrls.Length != 0;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _resource = parameter ?? throw new InvalidOperationException();
        ParseResource();
    }

    void ParseResource()
    {
        if (_resource is CurseForgeResource curseForgeResource)
        {
            IsHtmlDescription = true;

            IconUrl = curseForgeResource.IconUrl;
            Name = curseForgeResource.Name;
            Summary = curseForgeResource.Summary;
            WebLink = curseForgeResource.WebsiteUrl;
            Authors = string.Join(",", curseForgeResource.Authors);
            DownloadCount = curseForgeResource.DownloadCount;
            DateModified = curseForgeResource.DateModified;
            ScreenshotUrls = curseForgeResource.ScreenshotUrls.ToArray();
        }
        else if (_resource is ModrinthResource modrinthResource)
        {
            IconUrl = modrinthResource.IconUrl;
            Name = modrinthResource.Name;
            Summary = modrinthResource.Summary;
            WebLink = modrinthResource.WebLink;
            Authors = modrinthResource.Author;
            DownloadCount = modrinthResource.DownloadCount;
            DateModified = modrinthResource.DateModified;
            ScreenshotUrls = modrinthResource.ScreenshotUrls.ToArray();
        }
    }

    [RelayCommand]
    async Task DownloadResource() => await new DownloadResourceDialog() { DataContext = new DownloadResourceDialogViewModel(_resource, _navigationService) }.ShowAsync();

    [RelayCommand]
    public async Task MarkdownTextBlockLoadedEvent(object args)
    {
        var sender = args.As<MarkdownTextBlock, object>().sender;

        if (_resource is ModrinthResource modrinthResource)
        {
            var markdown = await _modrinthClient.GetResourceDescriptionAsync(modrinthResource.Id);
            App.DispatcherQueue.TryEnqueue(() => sender.Text = markdown);
        }
    }

    [RelayCommand]
    public async Task WebView2LoadedEvent(object args)
    {
        var sender = args.As<WebView2, object>().sender;

        if (_resource is CurseForgeResource curseForgeResource)
        {
            var body = await _curseForgeClient.GetResourceDescriptionAsync(curseForgeResource.Id);
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
    }
}
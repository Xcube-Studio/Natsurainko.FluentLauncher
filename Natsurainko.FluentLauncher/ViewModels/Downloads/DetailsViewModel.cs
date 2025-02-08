using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Resources;
using ReverseMarkdown;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DetailsViewModel : ObservableObject, INavigationAware
{
    private object _resource;

    private readonly INavigationService _navigationService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public DetailsViewModel(
        INavigationService navigationService,
        CurseForgeClient curseForgeClient,
        ModrinthClient modrinthClient,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        _navigationService = navigationService;
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
        _dialogs = dialogs;
    }

    [ObservableProperty]
    public partial string IconUrl { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; }

    [ObservableProperty]
    public partial string WebLink { get; set; }

    [ObservableProperty]
    public partial string Authors { get; set; }

    [ObservableProperty]
    public partial int DownloadCount { get; set; }

    [ObservableProperty]
    public partial DateTime DateModified { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasScreenshot))]
    public partial string[] ScreenshotUrls { get; set; }

    [ObservableProperty]
    public partial bool IsHtmlDescription { get; set; } = false;

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
    async Task DownloadResource() => await _dialogs.ShowAsync("DownloadResourceDialog", (_resource, _navigationService));

    [RelayCommand]
    public async Task MarkdownTextBlockLoadedEvent(object args)
    {
        MarkdownTextBlock markdownTextBlock = args.As<MarkdownTextBlock, object>().sender;

        if (_resource is ModrinthResource modrinthResource)
        {
            var markdown = await _modrinthClient.GetResourceDescriptionAsync(modrinthResource.Id);
            App.DispatcherQueue.TryEnqueue(() => markdownTextBlock.Text = markdown);
        }
        //else if (_resource is CurseForgeResource curseForgeResource)
        //{
        //    var body = await _curseForgeClient.GetResourceDescriptionAsync(curseForgeResource.Id);
        //    var config = new ReverseMarkdown.Config
        //    {
        //        UnknownTags = Config.UnknownTagsOption.Drop,
        //        GithubFlavored = true,
        //        SmartHrefHandling = true,
        //        WhitelistUriSchemes = []
        //    };

        //    var converter = new ReverseMarkdown.Converter(config);
        //    var markdown = converter.Convert(body);
        //    App.DispatcherQueue.TryEnqueue(() => markdownTextBlock.Text = markdown);
        //}
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

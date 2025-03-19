using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views.Downloads.Mods;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal partial class ModViewModel(
    GameService gameService,
    DownloadService downloadService,
    NotificationService notificationService,
    SearchProviderService searchProviderService,
    CurseForgeClient curseForgeClient,
    ModrinthClient modrinthClient,
    HttpClient httpClient) : PageVM<ModPage>, INavigationAware
{
    private object _modResource = null!;

    #region Basic Properties

    [ObservableProperty]
    public partial string IconUrl { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; }

    [ObservableProperty]
    public partial string WebLink { get; set; }

    [ObservableProperty]
    public partial ModAuthor[] Authors { get; set; }

    [ObservableProperty]
    public partial string[] Categories { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasScreenshot))]
    public partial string[] ScreenshotUrls { get; set; }

    [ObservableProperty]
    public partial string Source { get; set; }

    [ObservableProperty]
    public partial IEnumerable<object> Files { get; set; }

    #endregion

    [ObservableProperty]
    public partial bool ShowFiles { get; set; }

    [ObservableProperty]
    public partial bool ShowDescription { get; set; }

    [ObservableProperty]
    public partial bool ShowScreenShots { get; set; }

    [ObservableProperty]
    public partial bool Translated { get; set; } = false;

    [ObservableProperty]
    public partial bool TeachingTipOpen { get; set; } = false;

    #region Files

    [ObservableProperty]
    public partial IEnumerable<object> FilteredFiles { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedFile))]
    public partial object SelectedFile { get; set; }

    [ObservableProperty]
    public partial IEnumerable<string> FilesFilterVersions { get; set; }

    [ObservableProperty]
    public partial IEnumerable<string> FilesFilterLoaders { get; set; }

    [ObservableProperty]
    public partial string SelectedVersion { get; set; }

    [ObservableProperty]
    public partial string SelectedLoader { get; set; }

    [ObservableProperty]
    public partial bool LoadingFiles { get; set; }

    [ObservableProperty]
    public partial bool LoadFilesFailed { get; set; } = false;

    #endregion

    #region Description

    [ObservableProperty]
    public partial bool LoadingDescription { get; set; }

    [ObservableProperty]
    public partial bool LoadDescriptionFailed { get; set; } = false;

    [ObservableProperty]
    public partial bool IsHtml { get; set; } = false;

    [ObservableProperty]
    public partial bool IsMarkdown { get; set; } = false;

    public string DescriptionContent { get; set; }

    #endregion

    public bool HasMinecraftDataFolder => gameService.ActiveMinecraftFolder != null;

    public bool HasCurrentInstance => gameService.ActiveGame != null;

    public bool HasScreenshot => ScreenshotUrls != null && ScreenshotUrls.Length != 0;

    public bool IsSelectedFile => SelectedFile != null;

    partial void OnSelectedLoaderChanged(string value) => UpdateFilteredFiles();

    partial void OnSelectedVersionChanged(string value) => UpdateFilteredFiles();

    partial void OnSelectedFileChanged(object value)
    {
        if (TeachingTipOpen)
            TeachingTipOpen = false;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        searchProviderService.OccupyQueryReceiver(this, query => 
            GlobalNavigate("ModsDownload/Navigation", query));

        if (parameter is CurseForgeResource curseForgeResource)
        {
            IconUrl = curseForgeResource.IconUrl;
            Name = curseForgeResource.Name;
            Summary = curseForgeResource.Summary;
            WebLink = curseForgeResource.WebsiteUrl;
            Authors = [.. curseForgeResource.Authors.Select(author => new ModAuthor(author, $"https://www.curseforge.com/members/{author}"))];
            Categories = [.. curseForgeResource.Categories];
            ScreenshotUrls = [.. curseForgeResource.ScreenshotUrls];
            Source = "CurseForge";

            _modResource = curseForgeResource;
        }
        else if (parameter is ModrinthResource modrinthResource)
        {
            IconUrl = modrinthResource.IconUrl;
            Name = modrinthResource.Name;
            Summary = modrinthResource.Summary;
            WebLink = modrinthResource.WebLink;
            Authors = [new ModAuthor(modrinthResource.Author, $"https://modrinth.com/user/{modrinthResource.Author}")];
            Categories = [.. modrinthResource.Categories];
            ScreenshotUrls = [.. modrinthResource.ScreenshotUrls];
            Source = "Modrinth";

            _modResource = modrinthResource;
        }
        else throw new NotImplementedException();

        TryLoadFiles();
        TryLoadDescription();
        TryGetLocalizedSummary();
    }

    protected override void OnLoading()
    {
        this.Page.DescriptionMarkdown.Loaded += (_, _) =>
            Page.DescriptionMarkdown.Text = DescriptionContent;

        this.Page.DescriptionWebView2.Loaded += async (_, _) =>
        {
            WebView2 webView2 = this.Page.DescriptionWebView2;
            await webView2.EnsureCoreWebView2Async();

            webView2.CoreWebView2.Profile.PreferredColorScheme = webView2.ActualTheme == ElementTheme.Dark 
                ? CoreWebView2PreferredColorScheme.Dark 
                : CoreWebView2PreferredColorScheme.Light;
            
            string body = $"<meta name=\"color-scheme\" content=\"{(webView2.ActualTheme == ElementTheme.Dark ? "dark light" : "light dark")}\">\r\n" 
                + "<style>img{width:auto;height:auto;max-width:100%;max-height:100%;}</style>\r\n" 
                + $"<div id='container'>{DescriptionContent}</div>";

            webView2.MinHeight = DescriptionContent.Length / (webView2.ActualWidth / 8) * 14;
            webView2.NavigateToString(body);

            await Task.Delay(2000);

            var script = "eval(document.getElementById('container').getBoundingClientRect().height.toString());";
            var heightString = await webView2.ExecuteScriptAsync(script);

            if (double.TryParse(heightString, out double height))
                webView2.MinHeight = height + 30;
        };
    }

    [RelayCommand]
    void Download(int option)
    {
        if (!IsSelectedFile)
        {
            TeachingTipOpen = true;
            return;
        }

        string fileName;
        string savePath;

        if (SelectedFile is ModrinthFile modrinthFile)
            fileName = modrinthFile.FileName;
        else if (SelectedFile is CurseForgeFile curseForgeFile)
            fileName = curseForgeFile.FileName;
        else return;

        switch (option)
        {
            case 0:
                SaveFileDialog saveFileDialog = new()
                {
                    FileName = fileName,
                    InitialDirectory = gameService.ActiveMinecraftFolder ?? string.Empty,
                };

                if (saveFileDialog.ShowDialog().GetValueOrDefault())
                    savePath = new FileInfo(saveFileDialog.FileName).DirectoryName;
                else return;
                break;
            case 1:
                savePath = gameService.ActiveMinecraftFolder;
                break;
            case 2:
                savePath = gameService.ActiveGame.GetModsDirectory();
                break;
            default:
                return;
        }

        downloadService.DownloadModFile(SelectedFile, savePath);
        notificationService.NotifyWithoutContent(LocalizedStrings.Notifications__AddDownloadTask, icon: "\ue896");
    }

    async void TryGetLocalizedSummary()
    {
        if (ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans" && ApplicationLanguages.PrimaryLanguageOverride != "zh-Hant")
            return;

        try
        {
            string baseUrl = "https://mod.mcimirror.top/translate/";

            if (_modResource is CurseForgeResource curseForgeResource)
                baseUrl += $"curseforge?modId={curseForgeResource.Id}";
            else if (_modResource is ModrinthResource modrinthResource)
                baseUrl += $"modrinth?project_id={modrinthResource.Id}";
            else return;

            string json = await httpClient.GetStringAsync(baseUrl);
            string translatedSummary = JsonNode.Parse(json)!["translated"]!.GetValue<string>();

            await Dispatcher.EnqueueAsync(() =>
            {
                Summary = translatedSummary;
                Translated = true;
            });
        }
        catch { }
    }

    async void TryLoadFiles()
    {
        await Dispatcher.EnqueueAsync(() => LoadingFiles = true);

        IEnumerable<object> files = [];
        List<string> loaders = [];
        List<string> versions = [];

        if (_modResource is CurseForgeResource curseForgeResource)
        {
            files = curseForgeResource.Files;
            foreach (var file in curseForgeResource.Files)
            {
                var loaderName = file.ModLoaderType.ToString();

                if (!loaders.Contains(loaderName))
                    loaders.Add(loaderName);

                if (!versions.Contains(file.McVersion))
                    versions.Add(file.McVersion);
            }
        }
        else if (_modResource is ModrinthResource modrinthResource)
        {
            try
            {
                var modrinthFiles = await modrinthClient.GetProjectVersionsAsync(modrinthResource.Id);
                files = modrinthFiles;

                foreach (var file in modrinthFiles)
                {
                    foreach (var loader in file.Loaders)
                        if (!loaders.Contains(loader))
                            loaders.Add(loader);

                    if (!versions.Contains(file.McVersion))
                        versions.Add(file.McVersion);
                }
            }
            catch
            {
                await Dispatcher.EnqueueAsync(() => LoadFilesFailed = true);
            }
        }

        await Dispatcher.EnqueueAsync(() =>
        {
            LoadingFiles = false;
            Files = files;

            FilesFilterLoaders = loaders;
            SelectedLoader = loaders.FirstOrDefault(string.Empty);
            FilesFilterVersions = versions;
            SelectedVersion = versions.FirstOrDefault(string.Empty);
        });
    }

    async void TryLoadDescription()
    {
        await Dispatcher.EnqueueAsync(() => LoadingDescription = true);

        try
        {
            if (_modResource is CurseForgeResource curseForgeResource)
            {
                DescriptionContent = await curseForgeClient.GetResourceDescriptionAsync(curseForgeResource.Id);
                await Dispatcher.EnqueueAsync(() => IsHtml = true);
            }
            else if (_modResource is ModrinthResource modrinthResource)
            {
                DescriptionContent = await modrinthClient.GetResourceDescriptionAsync(modrinthResource.Id);
                await Dispatcher.EnqueueAsync(() => IsMarkdown = true);
            }
        }
        catch
        {
            await Dispatcher.EnqueueAsync(() => LoadDescriptionFailed = true);
        }

        await Dispatcher.EnqueueAsync(() => LoadingDescription = false);
    }

    void UpdateFilteredFiles()
    {
        if (_modResource is CurseForgeResource)
        {
            FilteredFiles = [.. Files.Cast<CurseForgeFile>()
                .Where(f => f.McVersion == SelectedVersion)
                .Where(f => f.ModLoaderType.ToString() == SelectedLoader)];
        }
        else if (_modResource is ModrinthResource)
        {
            FilteredFiles = [.. Files.Cast<ModrinthFile>()
                .Where(f => f.McVersion == SelectedVersion)
                .Where(f => f.Loaders.Contains(SelectedLoader))];
        };
    }
}

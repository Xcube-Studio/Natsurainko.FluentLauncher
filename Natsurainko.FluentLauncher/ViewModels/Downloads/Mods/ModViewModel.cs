using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Resources;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal partial class ModViewModel : ObservableObject, INavigationAware
{
    private readonly GameService _gameService;
    private readonly DownloadService _downloadService;
    private readonly NotificationService _notificationService;
    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    private object _modResource = null!;

    public ModViewModel(
        GameService gameService, 
        DownloadService downloadService, 
        NotificationService notificationService,
        CurseForgeClient curseForgeClient, 
        ModrinthClient modrinthClient)
    {
        _gameService = gameService;
        _downloadService = downloadService;
        _notificationService = notificationService;
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
    }

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
    public partial string Authors { get; set; }

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
        if (parameter is CurseForgeResource curseForgeResource)
        {
            IconUrl = curseForgeResource.IconUrl;
            Name = curseForgeResource.Name;
            Summary = curseForgeResource.Summary;
            WebLink = curseForgeResource.WebsiteUrl;
            Authors = string.Join(",", curseForgeResource.Authors);
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
            Authors = modrinthResource.Author;
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
                    InitialDirectory = _gameService.ActiveMinecraftFolder ?? string.Empty,
                };

                if (saveFileDialog.ShowDialog().GetValueOrDefault())
                    savePath = new FileInfo(saveFileDialog.FileName).DirectoryName;
                else return;
                break;
            case 1:
                savePath = _gameService.ActiveMinecraftFolder;
                break;
            case 2:
                savePath = _gameService.ActiveGame.GetModsDirectory();
                break;
            default:
                return;
        }

        _downloadService.DownloadModFile(SelectedFile, savePath);
        _notificationService.NotifyWithoutContent(LocalizedStrings.Notifications__AddDownloadTask, icon: "\ue896");
    }

    [RelayCommand]
    void MarkdownTextBlockLoadedEvent(object args)
    {
        MarkdownTextBlock markdownTextBlock = args.As<MarkdownTextBlock, object>().sender;
        markdownTextBlock.Text = DescriptionContent;
    }

    [RelayCommand]
    async Task WebView2LoadedEvent(object args)
    {
        var sender = args.As<WebView2, object>().sender;
        string body = "<style>img{width:auto;height:auto;max-width:100%;max-height:100%;}</style>" + $"<div id='container'>{DescriptionContent}</div>";

        await sender.EnsureCoreWebView2Async();

        sender.CoreWebView2.Profile.PreferredColorScheme = sender.ActualTheme == ElementTheme.Dark ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light;
        body = $"<meta name=\"color-scheme\" content=\"{(sender.ActualTheme == ElementTheme.Dark ? "dark light" : "light dark")}\">\r\n" + body;

        sender.MinHeight = DescriptionContent.Length / (sender.ActualWidth / 8) * 14;
        sender.NavigateToString(body);

        await Task.Delay(2000);

        var script = "eval(document.getElementById('container').getBoundingClientRect().height.toString());";
        var heightString = await sender.ExecuteScriptAsync(script);

        if (double.TryParse(heightString, out double height))
            sender.MinHeight = height + 30;
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

            string json = await HttpUtils.HttpClient.GetStringAsync(baseUrl);
            string translatedSummary = JsonNode.Parse(json)!["translated"]!.GetValue<string>();

            await App.DispatcherQueue.EnqueueAsync(() =>
            {
                Summary = translatedSummary;
                Translated = true;
            });
        }
        catch { }
    }

    async void TryLoadFiles()
    {
        await App.DispatcherQueue.EnqueueAsync(() => LoadingFiles = true);

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
                var modrinthFiles = await _modrinthClient.GetProjectVersionsAsync(modrinthResource.Id);
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
                await App.DispatcherQueue.EnqueueAsync(() => LoadFilesFailed = true);
            }
        }

        await App.DispatcherQueue.EnqueueAsync(() =>
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
        await App.DispatcherQueue.EnqueueAsync(() => LoadingDescription = true);

        try
        {
            if (_modResource is CurseForgeResource curseForgeResource)
            {
                DescriptionContent = await _curseForgeClient.GetResourceDescriptionAsync(curseForgeResource.Id);
                await App.DispatcherQueue.EnqueueAsync(() => IsHtml = true);
            }
            else if (_modResource is ModrinthResource modrinthResource)
            {
                DescriptionContent = await _modrinthClient.GetResourceDescriptionAsync(modrinthResource.Id);
                await App.DispatcherQueue.EnqueueAsync(() => IsMarkdown = true);
            }
        }
        catch
        {
            await App.DispatcherQueue.EnqueueAsync(() => LoadDescriptionFailed = true);
        }

        await App.DispatcherQueue.EnqueueAsync(() => LoadingDescription = false);
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

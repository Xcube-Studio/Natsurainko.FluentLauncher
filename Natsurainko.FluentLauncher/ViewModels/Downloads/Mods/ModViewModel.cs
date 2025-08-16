using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views.Downloads.Mods;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal partial class ModViewModel(
    GameService gameService,
    DownloadService downloadService,
    INotificationService notificationService,
    SearchProviderService searchProviderService,
    CurseForgeClient curseForgeClient,
    ModrinthClient modrinthClient,
    HttpClient httpClient) : PageVM, INavigationAware
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

    [RelayCommand]
    void Download(int option)
    {
        if (!IsSelectedFile)
        {
            TeachingTipOpen = true;
            return;
        }

        string fileName = SelectedFile switch
        {
            CurseForgeFile curseForgeFile => curseForgeFile.FileName,
            ModrinthFile modrinthFile => modrinthFile.FileName,
            _ => throw new NotImplementedException()
        };

        string savePath = option switch
        {
            1 => gameService.ActiveMinecraftFolder,
            2 => gameService.ActiveGame.GetModsDirectory(),
            _ => string.Empty
        };

        if (option == 0)
        {
            SaveFileDialog saveFileDialog = new()
            {
                FileName = fileName,
                InitialDirectory = gameService.ActiveMinecraftFolder ?? string.Empty
            };

            if (!saveFileDialog.ShowDialog().GetValueOrDefault())
                return;

            savePath = new FileInfo(saveFileDialog.FileName).DirectoryName;
        }

        if (SelectedFile is CurseForgeFile)
            downloadService.DownloadModFile((CurseForgeFile)SelectedFile, savePath);
        else if (SelectedFile is ModrinthFile)
            downloadService.DownloadModFile((ModrinthFile)SelectedFile, savePath);

        notificationService.ModDownloadTaskCreated(fileName);
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

internal static partial class ModViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__TaskCreated_ModDownload", Message = "{fileName}")]
    public static partial void ModDownloadTaskCreated(this INotificationService notificationService, string fileName);
}
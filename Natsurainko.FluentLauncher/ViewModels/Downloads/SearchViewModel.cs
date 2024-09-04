using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class SearchViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly CacheInterfaceService _cacheInterfaceService;
    private readonly SearchProviderService _searchProviderService;
    private readonly NotificationService _notificationService;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public SearchViewModel(
        INavigationService navigationService,
        GameService gameService,
        CacheInterfaceService cacheInterfaceService,
        SearchProviderService searchProviderService,
        NotificationService notificationService,
        CurseForgeClient curseForgeClient,
        ModrinthClient modrinthClient)
    {
        _navigationService = navigationService;
        _gameService = gameService;
        _cacheInterfaceService = cacheInterfaceService;
        _searchProviderService = searchProviderService;
        _notificationService = notificationService;

        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptySearchText))]
    private string searchText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearchMinecraft))]
    private int resourceType;

    [ObservableProperty]
    private int resourceSource;

    [ObservableProperty]
    private IEnumerable<string> resourceVersions;

    [ObservableProperty]
    private string selectedVersion;

    [ObservableProperty]
    private IEnumerable<object> searchResult;

    [ObservableProperty]
    public bool enableVersionFilter;

    public bool IsSearchMinecraft => ResourceType == 1;

    public bool IsEmptySearchText => string.IsNullOrEmpty(SearchText);

    internal static SearchOptions _lastSearchOptions;
    private VersionManifestItem[] VersionManifestItems;
    private string VersionManifestJson;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        parameter ??= _lastSearchOptions;

        if (parameter is not SearchOptions options)
            return;

        _lastSearchOptions = options;
        ResourceType = options.ResourceType;
        SearchText = options.SearchText;
        ResourceSource = options.ResourceSource;

        _cacheInterfaceService.RequestStringAsync(
            _cacheInterfaceService.VersionManifest,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParseVersionManifestTask,
            "cache-interfaces\\piston-meta.mojang.com\\version_manifest_v2.json")
        .ContinueWith(ParseVersionManifestTask)
        .ContinueWith(_ => SearchTask());
    }

    void ParseVersionManifestTask(Task<string> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        string versionManifestJson = task.Result;
        if (string.IsNullOrEmpty(versionManifestJson) || VersionManifestJson == versionManifestJson)
            return;

        var manifestItems = JsonNode.Parse(versionManifestJson)
            .Deserialize<VersionManifestJsonObject>().Versions.ToArray();

        VersionManifestItems = manifestItems;
        VersionManifestJson = versionManifestJson;

        var resourceVersions = manifestItems.Where(x => x.Type == "release").Select(x => x.Id).ToList();

        App.DispatcherQueue.TryEnqueue(() =>
        {
            SelectedVersion = resourceVersions.First();
            ResourceVersions = resourceVersions;
        });
    }

    async Task SearchTask()
    {
        var version = EnableVersionFilter ? SelectedVersion : null;

        ObservableCollection<object> searchResult = [];
        App.DispatcherQueue.TryEnqueue(() => SearchResult = searchResult);

        if (ResourceType == 0)
        {
            if (ResourceSource == 0)
            {
                foreach (var obj in VersionManifestItems.Where(x => x.Id.Contains(SearchText)).Take(3))
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }

            if (ResourceSource == 0 || ResourceSource == 1)
            {
                var curseForgeResources = await _curseForgeClient.SearchResourcesAsync(SearchText, version: version);
                if (ResourceSource == 0) curseForgeResources = curseForgeResources.Take(3);

                foreach (var obj in curseForgeResources)
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }

            if (ResourceSource == 0 || ResourceSource == 2)
            {
                var modrinthResources = await _modrinthClient.SearchResourcesAsync(SearchText, version: version);
                if (ResourceSource == 0) modrinthResources = modrinthResources.Take(3);

                foreach (var obj in modrinthResources)
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }
        }

        if (ResourceType == 1)
        {
            foreach (var obj in VersionManifestItems.Where(x => x.Id.Contains(SearchText)))
                App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
        }

        if (ResourceType == 2 || ResourceType == 3)
        {
            if (ResourceSource == 0 || ResourceSource == 1)
            {
                foreach (var obj in await _curseForgeClient.SearchResourcesAsync(SearchText, ResourceType == 2 ? CurseForgeResourceType.ModPack : CurseForgeResourceType.McMod, version: version))
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }

            if (ResourceSource == 0 || ResourceSource == 2)
            {
                foreach (var obj in await _modrinthClient.SearchResourcesAsync(SearchText, ResourceType == 2 ? ModrinthResourceType.ModPack : ModrinthResourceType.McMod, version: version))
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }
        }
    }

    void QueryReceiver(string searchText)
    {
        SearchText = searchText;
        Task.Run(SearchTask).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {

            }
        });
    }

    IEnumerable<SearchProviderService.Suggestion> ProviderSuggestions(string searchText)
    {
        if (ResourceType == 0)
        {
            foreach (var item in VersionManifestItems.Where(x => x.Id.Contains(searchText)).Take(2))
            {
                yield return SuggestionHelper.FromVersionManifestItem(item,
                    ResourceUtils.GetValue("SearchSuggest", "_D2"),
                    () => _navigationService.Parent.NavigateTo("Cores/Install", item));
            }
        }

        if (ResourceType == 1)
        {
            foreach (var item in VersionManifestItems.Where(x => x.Id.Contains(searchText)).Take(5))
            {
                yield return SuggestionHelper.FromVersionManifestItem(item,
                    ResourceUtils.GetValue("SearchSuggest", "_D2"),
                    () => _navigationService.Parent.NavigateTo("Cores/Install", item));
            }
        }
    }

    [RelayCommand]
    async Task DownloadResource(object resource) => await new DownloadResourceDialog() { DataContext = new DownloadResourceDialogViewModel(resource, _navigationService) }.ShowAsync();

    [RelayCommand]
    void ResourceDetails(object resource) => _navigationService.NavigateTo("Download/Details", resource);

    [RelayCommand]
    public void GoToSettings() => _navigationService.Parent.NavigateTo("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    public void CoreInstallWizard(VersionManifestItem manifestItem)
    {
        if (string.IsNullOrEmpty(_gameService.ActiveMinecraftFolder))
        {
            _notificationService.NotifyWithSpecialContent(
                ResourceUtils.GetValue("Notifications", "_NoMinecraftFolder"),
                "NoMinecraftFolderNotifyTemplate",
                GoToSettingsCommand, "\uE711");

            return;
        }

        _navigationService.Parent.NavigateTo("Cores/Install", manifestItem);
    }

    [RelayCommand]
    void Loaded()
    {
        if (_searchProviderService.QueryReceiverOwner != this)
            _searchProviderService.OccupyQueryReceiver(this, QueryReceiver);

        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);
    }

    [RelayCommand]
    void Unloaded()
    {
        _searchProviderService.UnregisterSuggestionProvider(this);
    }
}

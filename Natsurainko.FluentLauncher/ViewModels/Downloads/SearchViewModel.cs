using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Nrk.FluentCore.Management.Downloader.Data;
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
    private readonly CacheInterfaceService _cacheInterfaceService;
    private readonly SearchProviderService _searchProviderService;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public SearchViewModel(
        INavigationService navigationService,
        CacheInterfaceService cacheInterfaceService,
        SearchProviderService searchProviderService,
        CurseForgeClient curseForgeClient,
        ModrinthClient modrinthClient)
    {
        _navigationService = navigationService;
        _cacheInterfaceService = cacheInterfaceService;
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
        _searchProviderService = searchProviderService;
    }

    ~SearchViewModel()
    {
        ResourceVersions = null;
        VersionManifestItems = null;
        SearchResult = null; 

        VersionManifestJson = null;

        GC.Collect();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptySearchText))]
    private string searchText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearchMinecraft))]
    private int resourceType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllSource))]
    private int resourceSource;

    [ObservableProperty]
    private IEnumerable<string> resourceVersions;

    [ObservableProperty]
    private string selectedVersion;

    [ObservableProperty]
    private IEnumerable<object> searchResult;

    public bool IsSearchMinecraft => ResourceType == 1;

    public bool IsAllSource => ResourceSource == 0;

    public bool IsEmptySearchText => string.IsNullOrEmpty(SearchText);

    private VersionManifestItem[] VersionManifestItems;
    private string VersionManifestJson;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is not SearchOptions options)
            return;

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
            .Deserialize<VersionManifestJsonEntity>().Versions.ToArray();

        VersionManifestItems = manifestItems;
        VersionManifestJson = versionManifestJson;

        var resourceVersions = manifestItems.Where(x => x.Type == "release").Select(x => x.Id).ToList();

        App.DispatcherQueue.TryEnqueue(() =>
        {
            SelectedVersion = resourceVersions.First();
            ResourceVersions = resourceVersions;
        });
    }

    async void SearchTask()
    {
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
                var curseForgeResources = await _curseForgeClient.SearchResourcesAsync(SearchText);
                if (ResourceSource == 0) curseForgeResources = curseForgeResources.Take(3);

                foreach (var obj in curseForgeResources)
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }

            if (ResourceSource == 0 || ResourceSource == 2)
            {
                var modrinthResources = await _modrinthClient.SearchResourcesAsync(SearchText);
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
                foreach (var obj in (await _curseForgeClient.SearchResourcesAsync(SearchText, ResourceType == 2 ? CurseForgeResourceType.ModPack : CurseForgeResourceType.McMod)))
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }

            if (ResourceSource == 0 || ResourceSource == 2)
            {
                foreach (var obj in (await _modrinthClient.SearchResourcesAsync(SearchText, ResourceType == 2 ? ModrinthResourceType.ModPack : ModrinthResourceType.McMod)))
                    App.DispatcherQueue.TryEnqueue(() => searchResult.Add(obj));
            }
        }
    }

    void QueryReceiver(string searchText)
    {
        SearchText = searchText;
        Task.Run(SearchTask);
    }

    IEnumerable<SearchProviderService.Suggestion> ProviderSuggestions(string searchText)
    {
        if (ResourceType == 0)
        {
            foreach (var item in VersionManifestItems.Where(x => x.Id.Contains(searchText)).Take(2))
            {
                yield return new SearchProviderService.Suggestion
                {
                    Title = item.Id,
                    Description = "Suggestions from the current page",
                    SuggestionIconType = SearchProviderService.SuggestionIconType.UriIcon,
                    Icon = string.Format("ms-appx:///Assets/Icons/{0}.png", item.Type switch
                    {
                        "release" => "grass_block_side",
                        "snapshot" => "crafting_table_front",
                        "old_beta" => "dirt_path_side",
                        "old_alpha" => "dirt_path_side",
                        _ => "grass_block_side"
                    }),
                    InvokeAction = () =>
                    {

                    }
                };
            }
        }

        if (ResourceType == 1)
        {
            foreach (var item in VersionManifestItems.Where(x => x.Id.Contains(searchText)).Take(5))
            {
                yield return new SearchProviderService.Suggestion
                {
                    Title = item.Id,
                    Description = "Suggestions from the current page",
                    SuggestionIconType = SearchProviderService.SuggestionIconType.UriIcon,
                    Icon = string.Format("ms-appx:///Assets/Icons/{0}.png", item.Type switch
                    {
                        "release" => "grass_block_side",
                        "snapshot" => "crafting_table_front",
                        "old_beta" => "dirt_path_side",
                        "old_alpha" => "dirt_path_side",
                        _ => "grass_block_side"
                    }),
                    InvokeAction = () =>
                    {

                    }
                };
            }
        }

    }

    [RelayCommand]
    void Loaded()
    {
        if (_searchProviderService.QueryReceiverOwner != typeof(SearchViewModel))
            _searchProviderService.RegisterQueryReceiver(this, QueryReceiver);

        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);
    }

    [RelayCommand]
    void Unloaded() 
    {
        _searchProviderService.UnregisterQueryReceiver(this);
        _searchProviderService.UnregisterSuggestionProvider(this);
    }
}

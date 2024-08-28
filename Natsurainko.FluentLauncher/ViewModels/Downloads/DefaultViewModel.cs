using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models;
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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly CacheInterfaceService _cacheInterfaceService;
    private readonly SearchProviderService _searchProviderService;
    private readonly NotificationService _notificationService;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public DefaultViewModel(
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
    private PatchNoteData[] patchNoteDatas;

    [ObservableProperty]
    private VersionManifestItem[] versionManifestItems;

    [ObservableProperty]
    private CurseForgeResource[] curseForgeResources;

    [ObservableProperty]
    private ModrinthResource[] modrinthResources;

    [ObservableProperty]
    private bool loadingCurseForgeResource = true;

    [ObservableProperty]
    private bool loadingModrinthResource = true;

    private string PatchNotesJson;
    private string VersionManifestJson;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _cacheInterfaceService.RequestStringAsync(
            CacheInterfaceService.LauncherContentPatchNotes,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParsePatchNotesTask,
            "cache-interfaces\\launchercontent.mojang.com\\javaPatchNotes.json")
        .ContinueWith(ParsePatchNotesTask);


        _cacheInterfaceService.RequestStringAsync(
            _cacheInterfaceService.VersionManifest,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParseVersionManifestTask,
            "cache-interfaces\\piston-meta.mojang.com\\version_manifest_v2.json")
        .ContinueWith(ParseVersionManifestTask);

        _curseForgeClient.SearchResourcesAsync(string.Empty).ContinueWith(ParseCurseForgeTask);
        _modrinthClient.SearchResourcesAsync(string.Empty).ContinueWith(ParseModrinthTask);
    }

    void ParsePatchNotesTask(Task<string> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        string patchNotesJson = task.Result;
        if (string.IsNullOrEmpty(patchNotesJson) || PatchNotesJson == patchNotesJson)
            return;

        var patchNotes = JsonNode.Parse(patchNotesJson)!["entries"].AsArray().Select(node =>
        {
            var patchNote = node.Deserialize<PatchNoteData>();
            patchNote.ImageUrl = $"https://launchercontent.mojang.com{node["image"]!["url"].GetValue<string>()}";

            return patchNote;
        }).ToArray();

        var snapshot = patchNotes.Where(note => note.Type.Equals("snapshot")).Take(3);
        var release = patchNotes.Where(note => note.Type.Equals("release")).Take(2);

        var final = snapshot.Union(release).ToArray();

        PatchNotesJson = patchNotesJson;
        App.DispatcherQueue.TryEnqueue(() => PatchNoteDatas = final);
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
            .Deserialize<VersionManifestJsonObject>().Versions.Take(3).ToArray();

        App.DispatcherQueue.TryEnqueue(() => VersionManifestItems = manifestItems);

        VersionManifestJson = versionManifestJson;
    }

    void ParseCurseForgeTask(Task<IEnumerable<CurseForgeResource>> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        var curseForgeResources = task.Result.Reverse().Take(6).ToArray();

        App.DispatcherQueue.TryEnqueue(() =>
        {
            CurseForgeResources = curseForgeResources;
            LoadingCurseForgeResource = false;
        });
    }

    void ParseModrinthTask(Task<IEnumerable<ModrinthResource>> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        var modrinthResources = task.Result.ToArray();
        App.DispatcherQueue.TryEnqueue(() =>
        {
            ModrinthResources = modrinthResources;
            LoadingModrinthResource = false;
        });
    }

    IEnumerable<SearchProviderService.Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new SearchProviderService.Suggestion
        {
            Title = ResourceUtils.GetValue("SearchSuggest", "_T1").Replace("{searchText}", searchText),
            Description = ResourceUtils.GetValue("SearchSuggest", "_D1"),
            InvokeAction = () => _navigationService.NavigateTo("Download/Navigation", new SearchOptions
            {
                SearchText = searchText,
                ResourceType = 1
            })
        };

        yield return new SearchProviderService.Suggestion
        {
            Title = ResourceUtils.GetValue("SearchSuggest", "_T2").Replace("{searchText}", searchText),
            Description = ResourceUtils.GetValue("SearchSuggest", "_D1"),
            InvokeAction = () => _navigationService.NavigateTo("Download/Search", new SearchOptions
            {
                SearchText = searchText,
                ResourceSource = 1
            })
        };

        yield return new SearchProviderService.Suggestion
        {
            Title = ResourceUtils.GetValue("SearchSuggest", "_T3").Replace("{searchText}", searchText),
            Description = ResourceUtils.GetValue("SearchSuggest", "_D1"),
            InvokeAction = () => _navigationService.NavigateTo("Download/Search", new SearchOptions
            {
                SearchText = searchText,
                ResourceSource = 2
            })
        };

        foreach (var item in VersionManifestItems)
        {
            if (item.Id.Contains(searchText))
            {
                yield return SuggestionHelper.FromVersionManifestItem(item,
                    ResourceUtils.GetValue("SearchSuggest", "_D2"),
                    () => _navigationService.Parent.NavigateTo("CoreInstallWizardPage", item));
            }
        }
    }

    void QueryReceiver(string searchText) => _navigationService.NavigateTo("Download/Search", new SearchOptions { SearchText = searchText });

    [RelayCommand]
    void SearchAllMinecarft() => _navigationService.NavigateTo("Download/Search", new SearchOptions { ResourceType = 1 });

    [RelayCommand]
    void SearchMoreCurseForge() => _navigationService.NavigateTo("Download/Search", new SearchOptions { ResourceSource = 1 });

    [RelayCommand]
    void SearchMoreModrinth() => _navigationService.NavigateTo("Download/Search", new SearchOptions { ResourceSource = 2 });

    [RelayCommand]
    async Task DownloadResource(object resource) => await new DownloadResourceDialog() { DataContext = new DownloadResourceDialogViewModel(resource, _navigationService) }.ShowAsync();

    [RelayCommand]
    void ResourceDetails(object resource) => _navigationService.NavigateTo("Download/Details", resource);

    [RelayCommand]
    void GoToSettings() => _navigationService.Parent.NavigateTo("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    void FlipViewItemClicked(PatchNoteData patchNoteData)
    {
        if (string.IsNullOrEmpty(_gameService.ActiveMinecraftFolder))
        {
            _notificationService.NotifyWithSpecialContent(
                ResourceUtils.GetValue("Notifications", "_NoMinecraftFolder"),
                "NoMinecraftFolderNotifyTemplate",
                GoToSettingsCommand, "\uE711");

            return;
        }

        if (string.IsNullOrEmpty(VersionManifestJson))
            return;

        var manifestItem = JsonNode.Parse(VersionManifestJson)
            .Deserialize<VersionManifestJsonObject>().Versions.Where(x => x.Id.Equals(patchNoteData.Version)).FirstOrDefault();

        if (manifestItem != null)
            _navigationService.Parent.NavigateTo("CoreInstallWizardPage", manifestItem);
    }

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

        _navigationService.Parent.NavigateTo("CoreInstallWizardPage", manifestItem);
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

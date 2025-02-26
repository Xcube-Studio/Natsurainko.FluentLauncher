using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly GameService _gameService;
    private readonly CacheInterfaceService _cacheInterfaceService;
    private readonly SearchProviderService _searchProviderService;
    private readonly INavigationService _navigationService;
    private readonly NotificationService _notificationService;

    private string _versionManifestJson;

    public DefaultViewModel(
        CacheInterfaceService cacheInterfaceService, 
        SearchProviderService searchProviderService,
        INavigationService navigationService,
        GameService gameService,
        NotificationService notificationService)
    {
        _cacheInterfaceService = cacheInterfaceService;
        _searchProviderService = searchProviderService;
        _navigationService = navigationService;
        _gameService = gameService;
        _notificationService = notificationService;
    }

    public VersionManifestItem[] AllInstances { get; set; }

    [ObservableProperty]
    public partial VersionManifestItem[] FilteredInstances { get; set; }

    [ObservableProperty]
    public partial VersionManifestItem[] LatestInstances { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; } = true;

    [ObservableProperty]
    public partial bool LoadFailed { get; set; }

    [ObservableProperty]
    public partial bool Searched { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int ReleaseTypeFilterIndex { get; set; }

    partial void OnReleaseTypeFilterIndexChanged(int value) => SearchReceiveHandle(SearchQuery);

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _searchProviderService.OccupyQueryReceiver(this, SearchReceiveHandle);

        if (parameter is string searchInstanceId)
            SearchQuery = searchInstanceId;

        _cacheInterfaceService.RequestStringAsync(
            _cacheInterfaceService.VersionManifest,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParseVersionManifestTask,
            "cache-interfaces\\piston-meta.mojang.com\\version_manifest_v2.json")
        .ContinueWith(ParseVersionManifestTask);
    }

    [RelayCommand]
    void CardClick(VersionManifestItem instance)
    {
        if (string.IsNullOrEmpty(_gameService.ActiveMinecraftFolder))
        {
            _notificationService.NotifyWithSpecialContent(
                LocalizedStrings.Notifications__NoMinecraftFolder,
                "NoMinecraftFolderNotifyTemplate",
                GoToSettingsCommand, "\uE711");

            return;
        }

        _navigationService.NavigateTo("InstancesDownload/Install", instance);
    }

    [RelayCommand]
    void GoToSettings() => WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("Settings/Navigation", "Settings/Launch"));

    [RelayCommand]
    void ClearSearchQuery()
    {
        _searchProviderService.ClearSearchBox();
        SearchReceiveHandle(string.Empty);
    }

    async void ParseVersionManifestTask(Task<string> task)
    {
        if (task.IsFaulted || string.IsNullOrEmpty(task.Result))
        {
            await App.DispatcherQueue.EnqueueAsync(() =>
            {
                LoadFailed = true;
                Loading = false;
            });
            return;
        }

        try
        {
            string versionManifestJson = task.Result;

            if (!string.IsNullOrEmpty(_versionManifestJson) && versionManifestJson == _versionManifestJson)
                return;

            VersionManifestJsonObject versionManifest = JsonNode.Parse(versionManifestJson)
                .Deserialize(FLSerializerContext.Default.VersionManifestJsonObject);

            VersionManifestItem[] instances = versionManifest.Versions;
            VersionManifestItem[] latestInstances = [.. versionManifest.Latest.Select(kv => instances.First(i => i.Id == kv.Value))];

            if (string.IsNullOrEmpty(_versionManifestJson))
                _versionManifestJson = versionManifestJson;

            await App.DispatcherQueue.EnqueueAsync(() =>
            {
                AllInstances = instances;
                LatestInstances = latestInstances;
                SearchReceiveHandle(SearchQuery);
            });
        }
        catch (Exception e)
        {
            // TODO: Notify Exception
            await App.DispatcherQueue.EnqueueAsync(() => LoadFailed = true);
        }
        finally
        {
            await App.DispatcherQueue.EnqueueAsync(() => Loading = false);
        }
    }

    async void SearchReceiveHandle(string query)
    {
        string releaseType = ReleaseTypeFilterIndex switch
        {
            0 => "release",
            1 => "snapshot",
            2 => "old_beta",
            3 => "old_alpha",
            _ => throw new InvalidOperationException()
        };

        var filteredInstances = AllInstances?
            .Where(i => i.Type == releaseType)
            .Where(i => i.Id.Contains(query))
            .ToArray() ?? [];

        await App.DispatcherQueue.EnqueueAsync(() =>
        {
            FilteredInstances = filteredInstances;
            Searched = !string.IsNullOrEmpty(query);
            SearchQuery = query;
        });
    }
}

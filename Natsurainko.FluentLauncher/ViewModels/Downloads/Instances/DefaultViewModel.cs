using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;

internal partial class DefaultViewModel(
    CacheInterfaceService cacheInterfaceService,
    SearchProviderService searchProviderService,
    INavigationService navigationService,
    GameService gameService,
    INotificationService notificationService) : PageVM, INavigationAware
{
    private string _versionManifestJson = null!;

    public VersionManifestItem[] AllInstances { get; private set; } = null!;

    [ObservableProperty]
    public partial VersionManifestItem[] FilteredInstances { get; set; }

    [ObservableProperty]
    public partial VersionManifestItem LatestRelease { get; set; }

    [ObservableProperty]
    public partial VersionManifestItem LatestSnapshot { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQuery))]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int ReleaseTypeFilterIndex { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; } = true;

    public bool HasQuery => !string.IsNullOrEmpty(SearchQuery);

    partial void OnReleaseTypeFilterIndexChanged(int value) => SearchReceiveHandle(SearchQuery);

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        searchProviderService.OccupyQueryReceiver(this, SearchReceiveHandle);

        if (parameter is string searchInstanceId)
            SearchQuery = searchInstanceId;

        cacheInterfaceService.RequestStringAsync(
            cacheInterfaceService.VersionManifest,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParseVersionManifestTask,
            "cache-interfaces\\piston-meta.mojang.com\\version_manifest_v2.json")
        .ContinueWith(ParseVersionManifestTask!);
    }

    [RelayCommand]
    void CardClick(VersionManifestItem instance)
    {
        if (string.IsNullOrEmpty(gameService.ActiveMinecraftFolder))
        {
            notificationService.Show(new ActionNotification
            {
                Title = LocalizedStrings.Notifications__NoMinecraftDataFolder,
                Message = LocalizedStrings.Notifications__NoMinecraftDataFolderDescription,
                Type = NotificationType.Warning,
                GetActionButton = () => new HyperlinkButton()
                {
                    Command = this.GoToSettingsCommand,
                    Content = LocalizedStrings.Instances_DefaultPage__GoToSettings
                }
            });

            return;
        }

        navigationService.NavigateTo("InstancesDownload/Install", instance);
    }

    [RelayCommand]
    void GoToSettings() => GlobalNavigate("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    void ClearSearchQuery()
    {
        searchProviderService.ClearSearchBox();
        SearchReceiveHandle(string.Empty);
    }

    async void ParseVersionManifestTask(Task<string> task)
    {
        if (task.IsFaulted || string.IsNullOrEmpty(task.Result))
        {
            await App.DispatcherQueue.EnqueueAsync(() => Loading = false);
            notificationService.LoadInstancesFailed(task.Exception!);
            return;
        }

        try
        {
            string versionManifestJson = task.Result;

            if (!string.IsNullOrEmpty(_versionManifestJson) && versionManifestJson == _versionManifestJson)
                return;

            VersionManifestJsonObject versionManifest = JsonNode.Parse(versionManifestJson)
                .Deserialize(FLSerializerContext.Default.VersionManifestJsonObject)!;

            VersionManifestItem[] instances = versionManifest.Versions;
            VersionManifestItem[] latestInstances = [.. versionManifest.Latest.Select(kv => instances.First(i => i.Id == kv.Value))];
            var latestRelease = latestInstances.FirstOrDefault(i => i.Type == "release")!;
            var latestSnapshot = latestInstances.FirstOrDefault(i => i.Type == "snapshot")!;

            if (string.IsNullOrEmpty(_versionManifestJson))
                _versionManifestJson = versionManifestJson;

            await Dispatcher.EnqueueAsync(() =>
            {
                AllInstances = instances;
                LatestRelease = latestRelease;
                LatestSnapshot = latestSnapshot;
                SearchReceiveHandle(SearchQuery);
            });
        }
        catch (Exception ex)
        {
            notificationService.LoadInstancesFailed(ex);
        }
        finally
        {
            await Dispatcher.EnqueueAsync(() => Loading = false);
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

        await Dispatcher.EnqueueAsync(() =>
        {
            FilteredInstances = filteredInstances;
            SearchQuery = query;
        });
    }
}

internal static partial class DefaultViewModelNotifications
{
    [ExceptionNotification(Title = "Notifications__InsatnceListLoadFailed")]
    public static partial void LoadInstancesFailed(this INotificationService notificationService, Exception exception);
}
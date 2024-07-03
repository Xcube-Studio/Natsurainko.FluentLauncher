using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Management.Downloader.Data;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DownloadsViewModel : ObservableObject
{
    private readonly InterfaceCacheService _interfaceCacheService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;

    public DownloadsViewModel(
        InterfaceCacheService interfaceCacheService,
        INavigationService navigationService,
        GameService gameService,
        NotificationService notificationService)
    {
        _interfaceCacheService = interfaceCacheService;
        _navigationService = navigationService;
        _gameService = gameService;
        _notificationService = notificationService;

        Task.Run(async () =>
        {
            var publishDatas = await _interfaceCacheService.FetchMinecraftPublishes();

            App.DispatcherQueue.TryEnqueue(() =>
            {
                PrimaryPublishData = publishDatas[0];
                SecondaryPublishData = publishDatas[1];
            });
        });

        Task.Run(async () =>
        {
            var (McMods, ModPacks) = await _interfaceCacheService.FetchCurseForgeFeaturedResources();

            App.DispatcherQueue.TryEnqueue(() =>
            {
                CurseMcMods = McMods;
                CurseModPacks = ModPacks;
            });
        });
    }

    [ObservableProperty]
    private string searchBoxInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ComboBoxEnable))]
    [NotifyPropertyChangedFor(nameof(ModSearchProperty))]
    private int resourceType;

    [ObservableProperty]
    private PublishData primaryPublishData;

    [ObservableProperty]
    private PublishData secondaryPublishData;

    [ObservableProperty]
    private IEnumerable<CurseForgeResource> curseMcMods;

    [ObservableProperty]
    private IEnumerable<CurseForgeResource> curseModPacks;

    [ObservableProperty]
    private IEnumerable<string> versions;

    [ObservableProperty]
    private string selectedVersion;

    [ObservableProperty]
    private int selectedSource;

    public Visibility ModSearchProperty => ResourceType == 0 ? Visibility.Collapsed : Visibility.Visible;

    public bool ComboBoxEnable => ResourceType != 4;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ResourceType))
            if (ResourceType == 4)
                SelectedSource = 0;
    }

    [RelayCommand]
    public void NavigateResourcePage(object resource)
        => _navigationService.NavigateTo("ResourceItemPage", resource);

    [RelayCommand]
    public void SearchAllMinecraft()
        => _navigationService.NavigateTo("ResourcesSearchPage", new ResourceSearchData
        {
            SearchInput = string.Empty,
            ResourceType = 0,
            Source = SelectedSource,
            Version = SelectedVersion,
        });

    [RelayCommand]
    public void Search()
        => _navigationService.NavigateTo("ResourcesSearchPage", new ResourceSearchData
        {
            SearchInput = SearchBoxInput,
            ResourceType = ResourceType,
            Source = SelectedSource,
            Version = SelectedVersion,
        });

    [RelayCommand]
    public async Task DownloadMinecraft(PublishData resource)
    {
        IEnumerable<VersionManifestItem> manifestItems = (await _interfaceCacheService.FetchVersionManifest())
            .Where(manifestItem => manifestItem.Id.Equals(resource.Version));

        if (manifestItems.Any())
        {
            if (string.IsNullOrEmpty(_gameService.ActiveMinecraftFolder))
            {
                _notificationService.NotifyWithSpecialContent(
                    ResourceUtils.GetValue("Notifications", "_NoMinecraftFolder"),
                    "NoMinecraftFolderNotifyTemplate",
                    GoToSettingsCommand, "\uE711");

                return;
            }

            _navigationService.NavigateTo("CoreInstallWizardPage", manifestItems.First());
        }
        else _notificationService.NotifyWithoutContent(
            ResourceUtils.GetValue("Notifications", "_CoreNotInSourceList"),
            icon: "\uE9CE");
    }

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("SettingsNavigationPage", "LaunchSettingsPage");

}

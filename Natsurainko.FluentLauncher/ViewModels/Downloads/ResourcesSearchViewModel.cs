using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class ResourcesSearchViewModel : ObservableObject, INavigationAware
{
    private readonly InterfaceCacheService _interfaceCacheService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;

    public ResourcesSearchViewModel(
        InterfaceCacheService interfaceCacheServicel,
        INavigationService navigationService,
        GameService gameService,
        NotificationService notificationService)
    {
        _interfaceCacheService = interfaceCacheServicel;
        _navigationService = navigationService;
        _gameService = gameService;
        _notificationService = notificationService;
    }

    [ObservableProperty]
    private string searchBoxInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ComboBoxEnable))]
    [NotifyPropertyChangedFor(nameof(ModSearchProperty))]
    private int resourceType;

    [ObservableProperty]
    private IEnumerable<string> versions;

    [ObservableProperty]
    private string selectedVersion;

    [ObservableProperty]
    private int selectedSource;

    [ObservableProperty]
    private object searchedItems;

    public Visibility ModSearchProperty => ResourceType == 0 ? Visibility.Collapsed : Visibility.Visible;

    public bool ComboBoxEnable => ResourceType != 4;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is not ResourceSearchData searchData)
            throw new ArgumentException("Invalid parameter type");

        SearchBoxInput = searchData.SearchInput;
        ResourceType = searchData.ResourceType;
        SelectedSource = searchData.Source;
        SelectedVersion = searchData.Version;

        Search();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ResourceType))
            if (ResourceType == 4)
                SelectedSource = 0;
    }

    [RelayCommand]
    private void Search()
    {
        if (ResourceType == 0)
        {
            Task.Run(async () =>
            {
                IEnumerable<VersionManifestItem> manifestItems = await _interfaceCacheService.FetchVersionManifest();
                manifestItems = manifestItems.Where(manifestItem => manifestItem.Id.ToLower().Contains(SearchBoxInput.ToLower()));

                App.DispatcherQueue.TryEnqueue(() => SearchedItems = manifestItems);
            });

            return;
        }


        if (SelectedSource == 0)
        {
            Task.Run(() =>
            {
                var resources = _interfaceCacheService.CurseForgeClient.SearchResources(SearchBoxInput, ResourceType switch
                {
                    2 => CurseResourceType.ModPack,
                    3 => CurseResourceType.TexturePack,
                    4 => CurseResourceType.World,
                    _ => CurseResourceType.McMod
                });

                App.DispatcherQueue.TryEnqueue(() => SearchedItems = resources);
            });
        }
        else
        {
            Task.Run(() =>
            {
                var resources = _interfaceCacheService.ModrinthClient.SearchResources(SearchBoxInput, ResourceType switch
                {
                    2 => ModrinthResourceType.ModPack,
                    3 => ModrinthResourceType.Resourcepack,
                    _ => ModrinthResourceType.McMod
                });

                App.DispatcherQueue.TryEnqueue(() => SearchedItems = resources);
            });
        }
    }

    [RelayCommand]
    public void NavigateResourcePage(object resource) => _navigationService.NavigateTo("ResourceItemPage", resource);

    [RelayCommand]
    public void NavigateCoreInstallWizardPage(object resource)
    {
        if (string.IsNullOrEmpty(_gameService.ActiveMinecraftFolder))
        {
            _notificationService.NotifyWithSpecialContent(
                ResourceUtils.GetValue("Notifications", "_NoMinecraftFolder"),
                "NoMinecraftFolderNotifyTemplate",
                GoToSettingsCommand, "\uE711");

            return;
        }

        _navigationService.NavigateTo("CoreInstallWizardPage", resource);
    }

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("SettingsNavigationPage", "LaunchSettingsPage");
}

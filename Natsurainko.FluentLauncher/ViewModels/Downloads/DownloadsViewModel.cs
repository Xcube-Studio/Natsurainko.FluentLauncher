using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Downloads;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DownloadsViewModel : ObservableObject
{
    private readonly InterfaceCacheService _interfaceCacheService;
    private readonly INavigationService _navigationService;

    public DownloadsViewModel(InterfaceCacheService interfaceCacheService, INavigationService navigationService)
    {
        _interfaceCacheService = interfaceCacheService;
        _navigationService = navigationService;

        Task.Run(async () =>
        {
            var publishDatas = await _interfaceCacheService.FetchMinecraftPublishes();

            App.DispatcherQueue.TryEnqueue(() =>
            {
                PrimaryPublishData = publishDatas[0];
                SecondaryPublishData = publishDatas[1];
            });
        });

        Task.Run(() =>
        {
            _interfaceCacheService.FetchCurseForgeFeaturedResources(out var McMods, out var ModPacks);

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
    private IEnumerable<CurseResource> curseMcMods;

    [ObservableProperty]
    private IEnumerable<CurseResource> curseModPacks;

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
        => ShellPage.ContentFrame.Navigate(typeof(ResourceItemPage), resource);

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
    public void DownloadMinecraft(PublishData resource)
    {
        Task.Run(async () =>
        {
            IEnumerable<VersionManifestItem> manifestItems = await _interfaceCacheService.FetchVersionManifest();
            manifestItems = manifestItems.Where(manifestItem => manifestItem.Id.Equals(resource.Version));

            if (manifestItems.Any())
                App.DispatcherQueue.TryEnqueue(() => ShellPage.ContentFrame.Navigate(typeof(CoreInstallWizardPage), manifestItems.First()));
        });
    }

}

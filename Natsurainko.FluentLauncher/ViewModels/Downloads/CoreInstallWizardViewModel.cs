using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CoreInstallWizardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private WizardViewModelBase currentFrameDataContext;

    [ObservableProperty]
    private int navigationViewSelectIndex = 0;

    [ObservableProperty]
    private string[] breadcrumbBarItemsSource;

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly NotificationService _notificationService;
    private readonly DownloadService _downloadService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly InterfaceCacheService _interfaceCacheService;
    private VersionManifestItem _manifestItem;

    private Frame _contentFrame;
    private NavigationView _navigationView;


    public CoreInstallWizardViewModel(
        INavigationService navigationService,
        NotificationService notificationService,
        DownloadService downloadService,
        GameService gameService,
        InterfaceCacheService interfaceCacheService)
    {
        _notificationService = notificationService;
        _downloadService = downloadService;
        _navigationService = navigationService;
        _gameService = gameService;
        _interfaceCacheService = interfaceCacheService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _manifestItem = (VersionManifestItem)parameter;
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = grid.FindName("contentFrame") as Frame;
        _navigationView = grid.FindName("NavigationView") as NavigationView;

        BreadcrumbBarItemsSource = new string[]
        {
            ResourceUtils.GetValue("Downloads", "CoreInstallWizardPage", "_BreadcrumbBar_First"),
            _manifestItem.Id
        };

        CurrentFrameDataContext = new ChooseModLoaderViewModel(_manifestItem);

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    /// <summary>
    /// Next Button Command
    /// </summary>
    [RelayCommand]
    public void Next()
    {
        if (CurrentFrameDataContext.GetType().Equals(typeof(AdditionalOptionsViewModel)))
        {
            Finish();
            return;
        }

        _contentFrame.Content = null;

        _viewModelStack.Push(CurrentFrameDataContext);
        CurrentFrameDataContext = CurrentFrameDataContext.GetNextViewModel();

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

        NavigationViewSelectIndex++;
    }

    /// <summary>
    /// Back Button Command
    /// </summary>
    [RelayCommand]
    public void Previous()
    {
        _contentFrame.Content = null;

        CurrentFrameDataContext = _viewModelStack.Pop();

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

        NavigationViewSelectIndex--;
    }

    /// <summary>
    /// Cancel Button Command
    /// </summary>
    [RelayCommand]
    public void Cancel() => _navigationService.GoBack();

    [RelayCommand]
    void BreadcrumbBarClicked(object args)
    {
        var e = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>();

        if (e.args.Index.Equals(0))
            _navigationService.GoBack();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(NavigationViewSelectIndex))
        {
            var item = _navigationView.MenuItems.Cast<NavigationViewItem>().ToArray()[NavigationViewSelectIndex];

            _navigationView.SelectedItem = item;
            item.IsSelected = true;
        }
    }

    private void Finish()
    {
        var vm = (AdditionalOptionsViewModel)this.CurrentFrameDataContext;
        var installInfo = vm._coreInstallationInfo;
        var stepInstallMod = ResourceUtils.GetValue("Converters", "_ProgressItem_InstallMod");

        if (vm.EnabledFabricApi)
        {
            var file = installInfo.EnableIndependencyCore
                ? Path.Combine(_gameService.ActiveMinecraftFolder, "versions", installInfo.AbsoluteId, "mods", vm.FabricApi.FileName)
                : Path.Combine(_gameService.ActiveMinecraftFolder, "mods", vm.FabricApi.FileName);

            installInfo.AdditionalOptions.Add(new(@this =>
            {
                var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
                {
                    AbsolutePath = file,
                    Url = vm.FabricApi.Url
                },
                downloadSetting: new DownloadSetting
                {
                    EnableLargeFileMultiPartDownload = false
                },
                perSecondProgressChangedAction: @this.OnProgressChanged);

                downloadTask.Wait();
            }, stepInstallMod.Replace("${file}", vm.FabricApi.FileName)));
        }

        if (vm.EnabledOptiFabric)
        {
            var file = installInfo.EnableIndependencyCore
                ? Path.Combine(_gameService.ActiveMinecraftFolder, "versions", installInfo.AbsoluteId, "mods", vm.OptiFabric.FileName)
                : Path.Combine(_gameService.ActiveMinecraftFolder, "mods", vm.OptiFabric.FileName);

            installInfo.AdditionalOptions.Add(new(@this =>
            {
                var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
                {
                    AbsolutePath = file,
                    Url = _interfaceCacheService.CurseForgeClient.GetCurseFileDownloadUrl(vm.OptiFabric)
                },
                downloadSetting: new DownloadSetting
                {
                    EnableLargeFileMultiPartDownload = false
                },
                perSecondProgressChangedAction: @this.OnProgressChanged);

                downloadTask.Wait();
            }, stepInstallMod.Replace("${file}", vm.OptiFabric.FileName)));
        }

        _downloadService.InstallCore(installInfo);
        _navigationService.NavigateTo("ActivitiesNavigationPage", "DownloadTasksPage");
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Install;

internal partial class WizardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    public partial WizardViewModelBase CurrentFrameDataContext { get; set; }

    [ObservableProperty]
    public partial string[] BreadcrumbBarItemsSource { get; set; }

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly NotificationService _notificationService;
    private readonly DownloadService _downloadService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;

    private readonly CurseForgeClient _curseForgeClient;

    private VersionManifestItem _manifestItem;

    private Frame _contentFrame;

    public WizardViewModel(
        INavigationService navigationService,
        NotificationService notificationService,
        DownloadService downloadService,
        GameService gameService,
        CurseForgeClient curseForgeClient)
    {
        _notificationService = notificationService;
        _downloadService = downloadService;
        _navigationService = navigationService;
        _gameService = gameService;

        _curseForgeClient = curseForgeClient;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _manifestItem = (parameter as VersionManifestItem) ?? throw new InvalidDataException();
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = grid.FindName("contentFrame") as Frame;

        BreadcrumbBarItemsSource =
        [
            ResourceUtils.GetValue("Downloads", "CoreInstallWizardPage", "_BreadcrumbBar_First"),
            _manifestItem.Id
        ];

        CurrentFrameDataContext = new ChooseViewModel(_manifestItem);

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
        if (CurrentFrameDataContext.GetType().Equals(typeof(OptionsViewModel)))
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

    private void Finish()
    {
        var vm = (OptionsViewModel)this.CurrentFrameDataContext;
        var instanceInstallConfig = vm._installConfig;

        if (vm.EnabledFabricApi)
        {
            instanceInstallConfig.AdditionalResources.Add(
                new Services.Network.Data.GameResourceFile(Task.FromResult(vm.FabricApi.Url))
                {
                    FileName = vm.FabricApi.FileName,
                    Loaders = vm.FabricApi.Loaders,
                    Version = vm.FabricApi.McVersion
                });
        }

        if (vm.EnabledOptiFabric)
        {
            instanceInstallConfig.AdditionalResources.Add(
                new Services.Network.Data.GameResourceFile(_curseForgeClient.GetFileUrlAsync(vm.OptiFabric))
                {
                    FileName = vm.OptiFabric.FileName,
                    Loaders = [vm.OptiFabric.ModLoaderType.ToString()],
                    Version = vm.OptiFabric.McVersion
                });
        }

        _downloadService.InstallInstance(instanceInstallConfig);
        _navigationService.NavigateTo("Tasks/Download");
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CoreInstallWizardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private WizardViewModelBase currentFrameDataContext;

    [ObservableProperty]
    private int navigationViewSelectIndex = 0;

    public string[] PathItems { get; private set; }

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly NotificationService _notificationService;
    private readonly DownloadService _downloadService;
    private readonly INavigationService _navigationService;

    private VersionManifestItem _manifestItem;

    private Frame _contentFrame;
    private NavigationView _navigationView;


    public CoreInstallWizardViewModel(
        INavigationService navigationService, 
        NotificationService notificationService, 
        DownloadService downloadService)
    {
        _notificationService = notificationService;
        _downloadService = downloadService;
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _manifestItem = (VersionManifestItem)parameter;
        PathItems = new string[]
        {
            ResourceUtils.GetValue("Downloads", "CoreInstallWizardPage", "_BreadcrumbBar_First"),
            _manifestItem.Id
        };
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = grid.FindName("contentFrame") as Frame;
        _navigationView = grid.FindName("NavigationView") as NavigationView;

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
        var installInfo = ((AdditionalOptionsViewModel)this.CurrentFrameDataContext)._coreInstallationInfo;
        _downloadService.InstallCore(installInfo);
        _navigationService.NavigateTo("ActivitiesNavigationPage", "DownloadTasksPage");
    }
}

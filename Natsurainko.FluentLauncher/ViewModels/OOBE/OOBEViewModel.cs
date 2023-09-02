using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;
using System.Windows.Forms.VisualStyles;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

internal partial class OOBEViewModel : ObservableObject, INavigationAware, ISettingsViewModel
{
    #region ObservableProperties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    int currentPageIndex;

    #endregion

    #region Dependencies

    [SettingsProvider]
    private readonly SettingsService _settings;

    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;
    private readonly AccountService _accountService;

    #endregion

    private static readonly string[] OOBEPageKeys =
    {
        "OOBELanguagePage",
        "OOBEBasicPage",
        "OOBEAccountPage",
        "OOBEGetStartedPage"
    };


    private Type NextPage;

    public OOBEViewModel(
        INavigationService navigationService,
        SettingsService settings,
        GameService gameService,
        NotificationService notificationService,
        AccountService accountService)
    {
        _navigationService = navigationService;
        _settings = settings;
        _gameService = gameService;
        _notificationService = notificationService;
        _accountService = accountService;

        (this as ISettingsViewModel).InitializeSettings();

        //WeakReferenceMessenger.Default.Register<GuideNavigationMessage>(this, (r, m) =>
        //{
        //    CanNext = m.CanNext;
        //    NextPage = m.NextPage;
        //});
    }

    #region Navigation

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _navigationService.NavigateTo("OOBELanguagePage"); // Default page
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next(Frame frame)
    {
        CurrentPageIndex++;
        _navigationService.NavigateTo(OOBEPageKeys[CurrentPageIndex]);
    }

    bool CanNext()
    {
        // TODO: validation for NextCommand

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanBack))]
    public void Back(Frame frame)
    {
        _navigationService.GoBack();
        CurrentPageIndex--;
    }

    bool CanBack()
    {
        // Cannot go back on the first page
        if (CurrentPageIndex == 0)
            return false;

        return true;
    }

    #endregion
}

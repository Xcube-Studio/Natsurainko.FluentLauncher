using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Home;

internal partial class HomeViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<Account> Accounts { get; private set; }
    public ReadOnlyObservableCollection<ExtendedGameInfo> GameInfos { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    [NotifyPropertyChangedFor(nameof(NoAccountTag))]
    private Account activeAccount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchButtonTag))]
    private ExtendedGameInfo activeGameInfo;

    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly LaunchService _launchService;
    private readonly INavigationService _navigationService;

    private readonly string _coreNotSelected = ResourceUtils.GetValue("Home", "HomePage", "_NoCore");

    public HomeViewModel(GameService gameService, AccountService accountService, LaunchService launchService, INavigationService navigationService)
    {
        _accountService = accountService;
        _gameService = gameService;
        _launchService = launchService;
        _navigationService = navigationService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        GameInfos = _gameService.GameInfos;
        ActiveGameInfo = _gameService.ActiveGameInfo;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(this, (r, m) =>
        {
            HomeViewModel vm = r as HomeViewModel;
            vm.ActiveAccount = m.Value;
        });
    }

    public string LaunchButtonTag => ActiveGameInfo is null ? _coreNotSelected : ActiveGameInfo.Name;

    public Visibility NoAccountTag => ActiveAccount is null ? Visibility.Visible : Visibility.Collapsed;

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    [RelayCommand(CanExecute = nameof(CanExecuteLaunch))]
    private void Launch()
    {
        _navigationService.NavigateTo("ActivitiesNavigationPage", "LaunchTasksPage");
        Task.Run(() => _launchService.LaunchGame(ActiveGameInfo));
    }

    [RelayCommand]
    public void Account() => _navigationService.NavigateTo("SettingsNavigationPage", "AccountSettingsPage");

    [RelayCommand]
    public void Cores() => _navigationService.NavigateTo("CoresPage");

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActiveAccount) && ActiveAccount is not null)
            _accountService.Activate(ActiveAccount);

        if (e.PropertyName == nameof(ActiveGameInfo) && ActiveGameInfo is not null)
            _gameService.ActivateGameInfo(ActiveGameInfo);
    }

    private bool CanExecuteLaunch() => ActiveGameInfo is not null;
}

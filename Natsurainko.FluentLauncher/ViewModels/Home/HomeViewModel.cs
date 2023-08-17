using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

    private string _coreNotSelected = ResourceUtils.GetValue("Home", "HomePage", "_NoCore");

    public HomeViewModel(GameService gameService, AccountService accountService, LaunchService launchService)
    {
        _accountService = accountService;
        _gameService = gameService;
        _launchService = launchService;

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

    [RelayCommand]
    public void Launch()
    {
        _launchService.LaunchNewGame(ActiveGameInfo);
    }

    [RelayCommand]
    public void Account() => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Settings.NavigationPage), typeof(Views.Settings.AccountPage));

    [RelayCommand]
    public void Cores() => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Cores.CoresPage));

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActiveAccount) && ActiveAccount is not null)
            _accountService.Activate(ActiveAccount);

        if (e.PropertyName == nameof(ActiveGameInfo) && ActiveGameInfo is not null)
            _gameService.ActivateGameInfo(ActiveGameInfo);
    }
}

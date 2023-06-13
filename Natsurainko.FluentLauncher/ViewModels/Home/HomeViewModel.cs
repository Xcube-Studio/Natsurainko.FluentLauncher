using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Home;

partial class HomeViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<IAccount> Accounts { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    [NotifyPropertyChangedFor(nameof(NoAccountTag))]
    private IAccount activeAccount;


    private readonly SettingsService _settings;
    private readonly AccountService _accountService;


    public HomeViewModel(AccountService accountService, SettingsService settings)
    {
        _settings = settings;
        _accountService = accountService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(this, (r, m) =>
        {
            HomeViewModel vm = r as HomeViewModel;
            vm.ActiveAccount = m.Value;
        });

        if (!string.IsNullOrEmpty(_settings.CurrentGameFolder))
            Task.Run(() =>
            {
                var cores = new GameCoreLocator(_settings.CurrentGameFolder).GetGameCores();
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    GameCores.Clear();
                    foreach (var item in cores)
                    {
                        GameCores.Add(item);
                    }
                    CurrentGameCore = GameCores.Where(x => x.Id == _settings.CurrentGameCore).FirstOrDefault(cores.FirstOrDefault());
                });
            });

        PropertyChanged += HomeViewModel_PropertyChanged;
    }

    public ObservableCollection<GameCore> GameCores { get; } = new();

    [ObservableProperty]
    private GameCore currentGameCore;

    [ObservableProperty]
    private string launchButtonTag;

    public Visibility NoAccountTag => ActiveAccount is null ? Visibility.Visible : Visibility.Collapsed;

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    [RelayCommand]
    public Task Launch() => Task.Run(() => LaunchArrangement.StartNew(CurrentGameCore));

    [RelayCommand]
    public void Account() => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Settings.NavigationPage), typeof(Views.Settings.AccountPage));

    private void HomeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActiveAccount))
            _accountService.Activate(ActiveAccount);

        if (e.PropertyName == nameof(CurrentGameCore))
            _settings.CurrentGameCore = CurrentGameCore?.Id;

        if (e.PropertyName != nameof(LaunchButtonTag))
            LaunchButtonTag = CurrentGameCore == null
                ? "Core Not Selected"
                : CurrentGameCore.Id;
    }
}

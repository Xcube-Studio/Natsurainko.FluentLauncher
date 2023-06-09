using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Home;

public partial class HomeViewModel : ObservableObject, ISettingsViewModel
{
    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settings;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentAccount))]
    private IAccount currentAccount;

    [BindToSetting(Path = nameof(SettingsService.Accounts))]
    public ObservableCollection<IAccount> Accounts { get; private set; } = null!;

    #endregion

    public HomeViewModel(SettingsService settings)
    {
        _settings = settings;
        Accounts = new(_settings.Accounts);

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

        (this as ISettingsViewModel).InitializeSettings();
        PropertyChanged += HomeViewModel_PropertyChanged;
    }

    public ObservableCollection<GameCore> GameCores { get; } = new();

    [ObservableProperty]
    private GameCore currentGameCore;

    [ObservableProperty]
    private string launchButtonTag;

    [ObservableProperty]
    private Visibility noAccountTag;

    [ObservableProperty]
    private Visibility accountTag;

    [RelayCommand]
    public Task Launch() => Task.Run(() => LaunchArrangement.StartNew(CurrentGameCore));

    [RelayCommand]
    public void Account() => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Settings.NavigationPage), typeof(Views.Settings.AccountPage));

    private void HomeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentGameCore))
            _settings.CurrentGameCore = CurrentGameCore?.Id;

        if (e.PropertyName != nameof(LaunchButtonTag))
            LaunchButtonTag = CurrentGameCore == null
                ? "Core Not Selected"
                : CurrentGameCore.Id;

        if (e.PropertyName != nameof(NoAccountTag))
            NoAccountTag = CurrentAccount == null
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (e.PropertyName != nameof(AccountTag))
            AccountTag = CurrentAccount == null
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Views.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

public partial class Home : ObservableObject
{
    public Home()
    {
        Accounts = new(App.Configuration.Accounts);
        CurrentAccount = App.Configuration.CurrentAccount;

        if (!string.IsNullOrEmpty(App.Configuration.CurrentGameFolder))
            Task.Run(() =>
            {
                var cores = new GameCoreLocator(App.Configuration.CurrentGameFolder).GetGameCores();
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    GameCores = new(cores);
                    CurrentGameCore = GameCores.Where(x => x.Id == App.Configuration.CurrentGameCore).FirstOrDefault();
                });
            });
    }

    [ObservableProperty]
    private ObservableCollection<GameCore> gameCores;

    [ObservableProperty]
    private IAccount currentAccount;

    [ObservableProperty]
    private ObservableCollection<IAccount> accounts;

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
    public void Account() => MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Settings.Navigation), typeof(Views.Pages.Settings.Account));

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(CurrentAccount))
            App.Configuration.CurrentAccount = CurrentAccount;

        if (e.PropertyName == nameof(CurrentGameCore))
            App.Configuration.CurrentGameCore = CurrentGameCore?.Id;

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

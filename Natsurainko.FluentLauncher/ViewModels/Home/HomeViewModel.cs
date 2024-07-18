﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Management;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Home;

internal partial class HomeViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<GameInfo> GameInfos { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    private Account activeAccount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DropDownButtonDisplayText))]
    private GameInfo activeGameInfo;

    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly LaunchService _launchService;
    private readonly INavigationService _navigationService;

    public HomeViewModel(GameService gameService, AccountService accountService, LaunchService launchService, INavigationService navigationService)
    {
        _accountService = accountService;
        _gameService = gameService;
        _launchService = launchService;
        _navigationService = navigationService;

        ActiveAccount = accountService.ActiveAccount;

        GameInfos = _gameService.Games;
        ActiveGameInfo = _gameService.ActiveGame;
    }

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    public string DropDownButtonDisplayText => ActiveGameInfo == null
        ? ResourceUtils.GetValue("Home", "HomePage", "_NoCore")
        : ActiveGameInfo.Name;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActiveGameInfo) && ActiveGameInfo is not null)
            _gameService.ActivateGame(ActiveGameInfo);
    }

    private bool CanExecuteLaunch() => ActiveGameInfo is not null;

    [RelayCommand(CanExecute = nameof(CanExecuteLaunch))]
    private async Task Launch()
    {
        _navigationService.NavigateTo("ActivitiesNavigationPage", "LaunchTasksPage");
        await Task.Run(() => _launchService.LaunchGame(ActiveGameInfo));
    }

    [RelayCommand]
    public void GoToAccount() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Account");

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Launch");
}

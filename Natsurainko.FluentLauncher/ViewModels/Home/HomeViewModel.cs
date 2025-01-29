using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Data;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Home;

internal partial class HomeViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<MinecraftInstance> MinecraftInstances { get; private set; }

    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly LaunchService _launchService;
    private readonly INavigationService _navigationService;
    private readonly SearchProviderService _searchProviderService;

    public HomeViewModel(
        GameService gameService,
        AccountService accountService,
        LaunchService launchService,
        INavigationService navigationService,
        SearchProviderService searchProviderService)
    {
        _accountService = accountService;
        _gameService = gameService;
        _launchService = launchService;
        _navigationService = navigationService;
        _searchProviderService = searchProviderService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        MinecraftInstances = _gameService.Games;
        ActiveMinecraftInstance = _gameService.ActiveGame;
    }

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    public string DropDownButtonDisplayText => ActiveMinecraftInstance == null
        ? LocalizedStrings.Home_HomePage__NoCore
        : ActiveMinecraftInstance.GetDisplayName();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    public partial Account ActiveAccount { get; set; }

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DropDownButtonDisplayText))]
    public partial MinecraftInstance ActiveMinecraftInstance { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActiveMinecraftInstance) && ActiveMinecraftInstance is not null)
            _gameService.ActivateGame(ActiveMinecraftInstance);
    }

    private bool CanExecuteLaunch() => ActiveMinecraftInstance is not null;

    [RelayCommand(CanExecute = nameof(CanExecuteLaunch))]
    private void Launch() => _launchService.LaunchFromUI(ActiveMinecraftInstance);

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Launch");

    IEnumerable<Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new Suggestion
        {
            Title = LocalizedStrings.SearchSuggest__T1.Replace("{searchText}", searchText),
            Description = LocalizedStrings.SearchSuggest__D1,
            InvokeAction = () => _navigationService.NavigateTo("Download/Navigation", new SearchOptions
            {
                SearchText = searchText,
                ResourceType = 1
            })
        };

        foreach (var item in MinecraftInstances)
        {
            if (item.InstanceId.Contains(searchText))
            {
                yield return SuggestionHelper.FromMinecraftInstance(item,
                    LocalizedStrings.SearchSuggest__D4,
                    () => _launchService.LaunchFromUI(item));
            }
        }
    }

    [RelayCommand]
    void Loaded()
    {
        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);
    }

    [RelayCommand]
    void Unloaded()
    {
        _searchProviderService.UnregisterSuggestionProvider(this);
    }
}

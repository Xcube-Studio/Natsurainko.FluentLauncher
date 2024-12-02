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

        ActiveAccount = accountService.ActiveAccount;

        MinecraftInstances = _gameService.Games;
        ActiveMinecraftInstance = _gameService.ActiveGame;
    }

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    public string DropDownButtonDisplayText => ActiveMinecraftInstance == null
        ? ResourceUtils.GetValue("Home", "HomePage", "_NoCore")
        : ActiveMinecraftInstance.GetDisplayName();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    private Account activeAccount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DropDownButtonDisplayText))]
    private MinecraftInstance activeMinecraftInstance;

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
    public void GoToAccount() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Account");

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Launch");

    IEnumerable<SearchProviderService.Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new SearchProviderService.Suggestion
        {
            Title = ResourceUtils.GetValue("SearchSuggest", "_T1").Replace("{searchText}", searchText),
            Description = ResourceUtils.GetValue("SearchSuggest", "_D1"),
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
                    ResourceUtils.GetValue("SearchSuggest", "_D4"), 
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

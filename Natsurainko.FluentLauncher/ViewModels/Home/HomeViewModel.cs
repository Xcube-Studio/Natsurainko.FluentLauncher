using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Data;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Home;

internal partial class HomeViewModel : ObservableRecipient, IRecipient<ActiveAccountChangedMessage>, IRecipient<TrackLaunchTaskChangedMessage>
{
    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly LaunchService _launchService;
    private readonly INavigationService _navigationService;
    private readonly SearchProviderService _searchProviderService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogService;

    private static LaunchTaskViewModel _trackingTask = null;

    public ReadOnlyObservableCollection<MinecraftInstance> MinecraftInstances { get; private set; }

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    public HomeViewModel(
        GameService gameService,
        AccountService accountService,
        LaunchService launchService,
        INavigationService navigationService,
        SearchProviderService searchProviderService,
        IDialogActivationService<ContentDialogResult> dialogService)
    {
        _accountService = accountService;
        _gameService = gameService;
        _launchService = launchService;
        _navigationService = navigationService;
        _searchProviderService = searchProviderService;
        _dialogService = dialogService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        MinecraftInstances = _gameService.Games;
        ActiveMinecraftInstance = _gameService.ActiveGame;

        IsActive = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    public partial Account ActiveAccount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DropDownButtonDisplayText))]
    public partial MinecraftInstance ActiveMinecraftInstance { get; set; }

    [ObservableProperty]
    public partial LaunchTaskViewModel TrackingTask { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchButtonText))]
    [NotifyPropertyChangedFor(nameof(LaunchButtonIcon))]
    public partial bool IsTrackingTask { get; set; }

    [ObservableProperty]
    public partial Vector3 LaunchingInfoGridVector3 { get; set; } = new(480, 0, 0);

    [ObservableProperty]
    public partial Vector3 InstanceSelectorGridVector3 { get; set; } = new();

    [ObservableProperty]
    public partial double LaunchingInfoGridOpacity { get; set; } = 0;

    [ObservableProperty]
    public partial double InstanceSelectorGridOpacity { get; set; } = 1;

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    public string DropDownButtonDisplayText => ActiveMinecraftInstance == null ? LocalizedStrings.Home_HomePage__NoCore : ActiveMinecraftInstance.GetDisplayName();

    public string LaunchButtonText => IsTrackingTask ? "Cancel Launching" : LocalizedStrings.Home_HomePage_LaunchButton_Text;

    public string LaunchButtonIcon => IsTrackingTask ? "\uEE95" : "\uF5B0";

    partial void OnIsTrackingTaskChanged(bool value)
    {
        if (IsTrackingTask)
        {
            InstanceSelectorGridVector3 = new Vector3(Convert.ToSingle(App.MainWindow.Width) + 120, 0, 0);
            LaunchingInfoGridVector3 = new Vector3(0, 0, 0);

            InstanceSelectorGridOpacity = 0;
            LaunchingInfoGridOpacity = 1;
        }
        else
        {
            LaunchingInfoGridVector3 = new Vector3(480, 0, 0);
            InstanceSelectorGridVector3 = new Vector3(0, 0, 0);

            InstanceSelectorGridOpacity = 1;
            LaunchingInfoGridOpacity = 0;
        }
    }

    partial void OnActiveMinecraftInstanceChanged(MinecraftInstance value)
    {
        if (value is not null)
        {
            _gameService.ActivateGame(value);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteLaunch))]
    private void Launch()
    {
        //=> _launchService.LaunchFromUI(ActiveMinecraftInstance);

        if (IsTrackingTask)
        {
            

            return;
        }

        _launchService.LaunchFromUIWithTrack(ActiveMinecraftInstance);
    }

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    public void GoToAccountSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Account");

    [RelayCommand]
    public async Task AddAccount() => await _dialogService.ShowAsync("AuthenticationWizardDialog");

    [RelayCommand]
    void Loaded()
    {
        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);

        App.MainWindow.SizeChanged += SizeChanged;

        if (_trackingTask != null && _trackingTask.TaskState == TaskState.Running)
        {
            IsTrackingTask = true;
            TrackingTask = _trackingTask;
        }
        else _trackingTask = null;
    }

    [RelayCommand]
    void Unloaded()
    {
        _searchProviderService.UnregisterSuggestionProvider(this);

        App.MainWindow.SizeChanged -= SizeChanged;
        IsActive = false;
    }

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

    void IRecipient<ActiveAccountChangedMessage>.Receive(ActiveAccountChangedMessage message)
    {
        ActiveAccount = message.Value;
    }

    void IRecipient<TrackLaunchTaskChangedMessage>.Receive(TrackLaunchTaskChangedMessage message)
    {
        _trackingTask = message.Value;
        App.DispatcherQueue.TryEnqueue(() =>
        {
            IsTrackingTask = message.Value != null;
            TrackingTask = message.Value;
        });
    }

    void SizeChanged(object s, WindowSizeChangedEventArgs e)
    {
        if (!IsTrackingTask) return;

        InstanceSelectorGridVector3 = new Vector3(Convert.ToSingle(App.MainWindow.Width) + 120, 0, 0);
    }

    bool CanExecuteLaunch() => ActiveMinecraftInstance is not null;
}

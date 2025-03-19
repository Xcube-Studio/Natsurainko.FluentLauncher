using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Home;

internal partial class HomeViewModel : PageVM, IRecipient<TrackLaunchTaskChangedMessage>
{
    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly LaunchService _launchService;
    private readonly SettingsService _settingsService;
    private readonly SearchProviderService _searchProviderService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogService;

    private bool _registeredListener = false;
    private static LaunchTaskViewModel _trackingTask = null;

    public ReadOnlyObservableCollection<MinecraftInstance> MinecraftInstances { get; private set; }

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    public HomeViewModel(
        GameService gameService,
        AccountService accountService,
        LaunchService launchService,
        SettingsService settingsService,
        SearchProviderService searchProviderService,
        IDialogActivationService<ContentDialogResult> dialogService)
    {
        _accountService = accountService;
        _gameService = gameService;
        _launchService = launchService;
        _settingsService = settingsService;
        _searchProviderService = searchProviderService;
        _dialogService = dialogService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        MinecraftInstances = _gameService.Games;
        ActiveMinecraftInstance = _gameService.ActiveGame;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountTag))]
    public partial Account ActiveAccount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstanceSelectorText))]
    public partial MinecraftInstance ActiveMinecraftInstance { get; set; }

    [ObservableProperty]
    public partial LaunchTaskViewModel TrackingTask { get; set; }

    [ObservableProperty]
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

    [ObservableProperty]
    public partial string LaunchButtonText { get; set; } = LocalizedStrings.Home_HomePage_LaunchButton_Text;

    public Visibility AccountTag => ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    public string InstanceSelectorText => ActiveMinecraftInstance == null 
        ? LocalizedStrings.Home_HomePage__NoInstanceSelected
        : ActiveMinecraftInstance.GetDisplayName();

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
            _gameService.ActivateGame(value);
    }

    partial void OnActiveAccountChanged(Account value) => _accountService.ActivateAccount(value);

    [RelayCommand(CanExecute = nameof(CanExecuteLaunch))]
    private void Launch()
    {
        if (!_settingsService.EnableHomeLaunchTaskTrack)
        {
            _launchService.LaunchFromUI(ActiveMinecraftInstance);
            return;
        }

        if (IsTrackingTask)
        {
            if (TrackingTask.ProcessLaunched)
                TrackingTask.KillProcess();
            else if (TrackingTask.CanCancel)
                TrackingTask.Cancel();

            return;
        }

        _launchService.LaunchFromUIWithTrack(ActiveMinecraftInstance);
    }

    [RelayCommand]
    void GoToInstancesManage() => GlobalNavigate("Instances/Navigation");

    [RelayCommand]
    void GoToAccountSettings() => GlobalNavigate("Settings/Navigation", "Settings/Account");

    [RelayCommand]
    async Task AddAccount() => await _dialogService.ShowAsync("AuthenticationWizardDialog");

    [RelayCommand]
    void Continue() => WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(null));

    [RelayCommand]
    void ShowDetails() => GlobalNavigate("Tasks/Launch");

    void TrackingTask_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ProcessLaunched")
            UpdateLaunchButtonText();
    }

    IEnumerable<Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new Suggestion
        {
            Title = LocalizedStrings.SearchSuggest__T1.Replace("{searchText}", searchText),
            Description = LocalizedStrings.SearchSuggest__D1,
            InvokeAction = () => GlobalNavigate("InstancesDownload/Navigation", searchText)
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

    protected override void OnLoaded()
    {
        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);

        App.MainWindow.SizeChanged += SizeChanged;

        if (_trackingTask != null && _trackingTask.TaskState == TaskState.Running)
        {
            TrackingTask = _trackingTask;
            IsTrackingTask = true;
            UpdateLaunchButtonText();

            TrackingTask.PropertyChanged += TrackingTask_PropertyChanged;
            _registeredListener = true;
        }
        else _trackingTask = null;
    }

    protected override void OnUnloaded()
    {
        _searchProviderService.UnregisterSuggestionProvider(this);

        if (_registeredListener)
        {
            TrackingTask.PropertyChanged -= TrackingTask_PropertyChanged;
            _registeredListener = false;
        }

        App.MainWindow.SizeChanged -= SizeChanged;
    }

    void IRecipient<TrackLaunchTaskChangedMessage>.Receive(TrackLaunchTaskChangedMessage message)
    {
        if (_registeredListener)
        {
            TrackingTask.PropertyChanged -= TrackingTask_PropertyChanged;
            _registeredListener = false;
        }

        _trackingTask = message.Value;

        Dispatcher.TryEnqueue(() =>
        {
            TrackingTask = message.Value;
            IsTrackingTask = message.Value != null;
            UpdateLaunchButtonText();

            if (IsTrackingTask)
            {
                TrackingTask.PropertyChanged += TrackingTask_PropertyChanged;
                _registeredListener = true;
            }
        });
    }

    void SizeChanged(object s, WindowSizeChangedEventArgs e)
    {
        if (!IsTrackingTask) return;

        InstanceSelectorGridVector3 = new Vector3(Convert.ToSingle(App.MainWindow.Width) + 120, 0, 0);
    }

    bool CanExecuteLaunch() => ActiveMinecraftInstance is not null;

    void UpdateLaunchButtonText()
    {
        if (IsTrackingTask)
        {
            if (TrackingTask.ProcessLaunched)
                LaunchButtonText = LocalizedStrings.Home_HomePage__KillProcess.Replace("Minecraft", TrackingTask.TaskTitle);
            else LaunchButtonText = LocalizedStrings.Home_HomePage__CancelLaunch.Replace("Minecraft", TrackingTask.TaskTitle);

            return;
        }

        LaunchButtonText = LocalizedStrings.Home_HomePage_LaunchButton_Text;
    }
}

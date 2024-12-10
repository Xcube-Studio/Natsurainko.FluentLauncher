using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AccountViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly AccountService _accountService;
    private readonly AuthenticationService _authenticationService;
    private readonly NotificationService _notificationService;
    private readonly INavigationService _navigationService;
    private readonly CacheSkinService _cacheSkinService;

    public AccountViewModel(
        SettingsService settingsService,
        AccountService accountService,
        AuthenticationService authenticationService,
        NotificationService notificationService,
        INavigationService navigationService,
        CacheSkinService cacheSkinService)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        _authenticationService = authenticationService;
        _notificationService = notificationService;
        _navigationService = navigationService;
        _cacheSkinService = cacheSkinService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableDemoUser))]
    private bool enableDemoUser;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.AutoRefresh))]
    private bool autoRefresh;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SkinFile))]
    [NotifyPropertyChangedFor(nameof(IsOfflineAccount))]
    private Account activeAccount;

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    public string SkinFile => _cacheSkinService.GetSkinFilePath(ActiveAccount);

    public bool IsOfflineAccount => ActiveAccount.Type == AccountType.Offline;

    //partial void OnActiveAccountChanged(Account value)
    //{
    //    if (value is not null) _accountService.ActivateAccount(value);
    //}

    [RelayCommand]
    public async Task Login() => await new AuthenticationWizardDialog().ShowAsync();

    [RelayCommand]
    public async Task Refresh()
    {
        await _accountService.RefreshAccountAsync(ActiveAccount).ContinueWith(task => 
        {
            if (task.IsFaulted)
                _notificationService.NotifyException("_AccountRefreshFailedTitle", task.Exception, "_AccountRefreshFailedDescription");
            else _notificationService.NotifyMessage(
                ResourceUtils.GetValue("Notifications", "_AccountRefreshedTitle"),
                ResourceUtils.GetValue("Notifications", "_AccountRefreshedDescription").Replace("${name}", _accountService.ActiveAccount.Name));
        });
    }

    [RelayCommand]
    public void Switch()
    {
        var switchAccountDialog = new SwitchAccountDialog
        {
            DataContext = App.Services.GetService<SwitchAccountDialogViewModel>()
        };
        _ = switchAccountDialog.ShowAsync();
    }

    [RelayCommand]
    public void OpenSkinFile()
    {
        if (!File.Exists(SkinFile))
            return;

        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{SkinFile}"));
    }

    [RelayCommand]
    public void GoToSkinPage() => _navigationService.NavigateTo("Settings/Account/Skin");
}
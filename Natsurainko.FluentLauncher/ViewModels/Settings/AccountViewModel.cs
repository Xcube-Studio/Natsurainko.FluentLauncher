using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
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
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    public AccountViewModel(
        SettingsService settingsService,
        AccountService accountService,
        AuthenticationService authenticationService,
        NotificationService notificationService,
        INavigationService navigationService,
        CacheSkinService cacheSkinService,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        _authenticationService = authenticationService;
        _notificationService = notificationService;
        _navigationService = navigationService;
        _cacheSkinService = cacheSkinService;
        _dialogs = dialogs;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableDemoUser))]
    public partial bool EnableDemoUser { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.AutoRefresh))]
    public partial bool AutoRefresh { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SkinFile))]
    [NotifyPropertyChangedFor(nameof(IsOfflineAccount))]
    public partial Account ActiveAccount { get; set; }
    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    public string SkinFile => _cacheSkinService.GetSkinFilePath(ActiveAccount);

    public bool IsOfflineAccount => ActiveAccount.Type == AccountType.Offline;

    partial void OnActiveAccountChanged(Account value)
    {
        //if (value is not null) _accountService.ActivateAccount(value);
    }

    [RelayCommand]
    public async Task Login() => await _dialogs.ShowAsync("AuthenticationWizardDialog");

    [RelayCommand]
    public async Task Refresh()
    {
        await _accountService.RefreshAccountAsync(ActiveAccount).ContinueWith(task => 
        {
            if (task.IsFaulted)
                _notificationService.NotifyException(
                    LocalizedStrings.Notifications__AccountRefreshFailedTitle,
                    task.Exception,
                    LocalizedStrings.Notifications__AccountRefreshFailedDescription);
            else
                _notificationService.NotifyMessage(
                    LocalizedStrings.Notifications__AccountRefreshedTitle,
                    LocalizedStrings.Notifications__AccountRefreshedDescription.Replace("${name}", _accountService.ActiveAccount.Name));
        });
    }

    [RelayCommand]
    public async Task Switch() => await _dialogs.ShowAsync("SwitchAccountDialog");

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
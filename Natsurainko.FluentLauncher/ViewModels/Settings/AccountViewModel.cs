﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Authentication;
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
    private readonly NotificationService _notificationService;
    private readonly INavigationService _navigationService;
    private readonly CacheSkinService _cacheSkinService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    public AccountViewModel(
        SettingsService settingsService,
        AccountService accountService,
        NotificationService notificationService,
        INavigationService navigationService,
        CacheSkinService cacheSkinService,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        _settingsService = settingsService;
        _accountService = accountService;
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
    [NotifyPropertyChangedFor(nameof(IsOfflineAccount))]
    public partial Account ActiveAccount { get; set; }

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    public bool IsOfflineAccount => ActiveAccount.Type == AccountType.Offline;

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
        string skinFilePath = GetSkinFilePath(ActiveAccount);
        if (!File.Exists(skinFilePath)) return;

        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{skinFilePath}"));
    }

    [RelayCommand]
    public void GoToSkinPage() => _navigationService.NavigateTo("Settings/Account/Skin");

    #region Converters Methods

    internal Visibility IsLoadLastRefreshCard(AccountType accountType) => accountType == AccountType.Microsoft ? Visibility.Visible : Visibility.Collapsed;

    internal string GetAccountTypeName(AccountType accountType)
    {
        string account = LocalizedStrings.Converters__Account;

        if (!ApplicationLanguages.PrimaryLanguageOverride.StartsWith("zh-"))
            account = " " + account;

        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft + account,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil + account,
            _ => LocalizedStrings.Converters__Offline + account,
        };
    }

    internal string TryGetLastRefreshTime(Account account)
    {
        if (account is MicrosoftAccount microsoftAccount)
        {
            return microsoftAccount.LastRefreshTime.ToLongTimeString()
                + ", " + microsoftAccount.LastRefreshTime.ToLongDateString();
        }

        return string.Empty;
    }

    internal string TryGetYggdrasilServerName(Account account)
    {
        if (account is YggdrasilAccount yggdrasilAccount)
        {
            if (yggdrasilAccount.MetaData.TryGetValue("server_name", out var serverName))
                return serverName;
        }

        return string.Empty;
    }

    internal string GetSkinFilePath(Account account) => _cacheSkinService.GetSkinFilePath(account);

    #endregion
}
using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

partial class AccountViewModel : SettingsViewModelBase, ISettingsViewModel
{
    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableDemoUser))]
    private bool enableDemoUser;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.AutoRefresh))]
    private bool autoRefresh;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseDeviceFlowAuth))]
    private bool useDeviceFlowAuth;

    #endregion


    public ReadOnlyObservableCollection<IAccount> Accounts { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRemoveVisible))]
    private IAccount currentAccount;

    public bool IsRemoveVisible => CurrentAccount is not null;

    private readonly AccountService _accountService;


    public AccountViewModel(SettingsService settingsService, AccountService accountService)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        Accounts = accountService.Accounts;
        
        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(this, (r, m) =>
        {
            AccountViewModel vm = r as AccountViewModel;
            vm.CurrentAccount = m.Value;
        });
        (this as ISettingsViewModel).InitializeSettings();
    }


    [RelayCommand]
    public void Remove()
    {
        _accountService.Remove(CurrentAccount);
    }

    [RelayCommand]
    public Task Login() => Task.Run(() =>
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var chooseAccountTypeDialog = new Views.Common.ChooseAccountTypeDialog
            {
                XamlRoot = Views.ShellPage._XamlRoot,
                DataContext = new Common.ChooseAccountTypeDialog { SetAccountAction = SetAccount }
            };
            await chooseAccountTypeDialog.ShowAsync();
        });
    });

    [RelayCommand]
    public Task Refresh() => Task.Run(async () =>
    {
        try
        {
            IAuthenticator authenticator = default;
            IAccount refreshedAccount = default;
            if (CurrentAccount.Type.Equals(AccountType.Microsoft))
            {
                var account = (MicrosoftAccount)CurrentAccount;
                authenticator = new MicrosoftAuthenticator(
                    account.RefreshToken, 
                    "0844e754-1d2e-4861-8e2b-18059609badb", 
                    "https://login.live.com/oauth20_desktop.srf",
                    AuthenticatorMethod.Refresh);
            }
            else if (CurrentAccount.Type.Equals(AccountType.Yggdrasil))
            {
                var account = (YggdrasilAccount)CurrentAccount;
                authenticator = new YggdrasilAuthenticator(
                    AuthenticatorMethod.Refresh,
                    account.AccessToken,
                    account.ClientToken,
                    yggdrasilServerUrl: account.YggdrasilServerUrl);
            }
            else if (CurrentAccount.Type.Equals(AccountType.Offline))
                authenticator = new OfflineAuthenticator(CurrentAccount.Name, CurrentAccount.Uuid);

            refreshedAccount = await authenticator.AuthenticateAsync();

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                _accountService.Remove(CurrentAccount);

#pragma warning disable CS0612 // Type or member is obsolete
                _accountService.AddAccount(refreshedAccount);
#pragma warning restore CS0612 // Type or member is obsolete
                CurrentAccount = refreshedAccount;
            });

            MessageService.ShowSuccess("Successfully refreshed Account", $"Welcome back, {refreshedAccount.Name}");
        }
        catch (Exception ex)
        {
            MessageService.ShowException(ex, "Failed to refresh account");
        }
    });

    private void SetAccount(IAccount account) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
#pragma warning disable CS0612 // Type or member is obsolete
        _accountService.AddAccount(account);
#pragma warning restore CS0612 // Type or member is obsolete
        CurrentAccount = account;

        MessageService.ShowSuccess($"Add {account.Type} Account Successfully", $"Welcome back, {account.Name}");
    });

}
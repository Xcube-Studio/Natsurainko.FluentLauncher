using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
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

    [BindToSetting(Path = nameof(SettingsService.Accounts))]
    public ObservableCollection<IAccount> Accounts { get; private set; }

    [ObservableProperty]
    //TODO: Add CurrentAccount to SettingsService [BindToSetting(Path = nameof(SettingsService.))]
    private IAccount currentAccount;

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

    [ObservableProperty]
    private Visibility removeVisibility;


    public AccountViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(RemoveVisibility))
            RemoveVisibility = CurrentAccount == null
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    [RelayCommand]
    public void Remove()
    {
        Accounts.Remove(CurrentAccount);
        CurrentAccount = Accounts.Any() ? Accounts[0] : null;

        OnPropertyChanged(nameof(Accounts));
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
                Accounts.Remove(CurrentAccount);
                CurrentAccount = null;

                Accounts.Add(refreshedAccount);
                CurrentAccount = refreshedAccount;

                OnPropertyChanged(nameof(Accounts));
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
        Accounts.Add(account);
        CurrentAccount = account;

        OnPropertyChanged(nameof(Accounts));

        MessageService.ShowSuccess($"Add {account.Type} Account Successfully", $"Welcome back, {account.Name}");
    });

}
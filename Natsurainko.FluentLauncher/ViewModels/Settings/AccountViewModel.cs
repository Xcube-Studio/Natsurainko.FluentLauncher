using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public partial class AccountViewModel : SettingViewModel
{
    public AccountViewModel() : base() { }

    protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RemoveVisibility))
            RemoveVisibility = CurrentAccount == null
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}

public partial class AccountViewModel
{
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
            var chooseAccountTypeDialog = new ChooseAccountTypeDialog
            {
                XamlRoot = Views.ShellPage._XamlRoot,
                DataContext = new Dialogs.ChooseAccountTypeDialog { SetAccountAction = SetAccount }
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

public partial class AccountViewModel
{
    [ObservableProperty]
    private Visibility removeVisibility;

    [ObservableProperty]
    private ObservableCollection<IAccount> accounts;

    [ObservableProperty]
    private IAccount currentAccount;

    [ObservableProperty]
    private bool enableDemoUser;

    [ObservableProperty]
    private bool autoRefresh;

    [ObservableProperty]
    private bool useDeviceFlowAuth;
}
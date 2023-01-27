using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Natsurainko.FluentLauncher.Views.Pages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Settings;

public partial class Account : SettingViewModel
{
    public Account() : base() { }

    protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RemoveVisibility))
            RemoveVisibility = CurrentAccount == null
                ? Visibility.Collapsed
                : Visibility.Visible;

        if (e.PropertyName != nameof(MaxFlyoutHeight))
            MaxFlyoutHeight = ExpandAccessToken ? 1000 : 100;
    }
}

public partial class Account
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
                XamlRoot = MainContainer._XamlRoot,
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

            MainContainer.ShowMessagesAsync(
                "Successfully refreshed Account",
                $"Welcome back, {refreshedAccount.Name}",
                severity: InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            MainContainer.ShowMessagesAsync(
                "Failed to refresh account",
                ex.ToString(),
                severity: InfoBarSeverity.Error,
                delay: 1000 * 15);
        }
    });

    private void SetAccount(IAccount account) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
        Accounts.Add(account);
        CurrentAccount = account;

        OnPropertyChanged(nameof(Accounts));

        MainContainer.ShowMessagesAsync(
            $"Add {account.Type} Account Successfully",
            $"Welcome back, {account.Name}",
            Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
    });
}

public partial class Account
{
    [ObservableProperty]
    private Visibility removeVisibility;

    [ObservableProperty]
    private int maxFlyoutHeight;

    [ObservableProperty]
    private bool expandAccessToken;

    [ObservableProperty]
    private ObservableCollection<IAccount> accounts;

    [ObservableProperty]
    private IAccount currentAccount;

    [ObservableProperty]
    private bool enableDemoUser;

    [ObservableProperty]
    private bool autoRefresh;
}
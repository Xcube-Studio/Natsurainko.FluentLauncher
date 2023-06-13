using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.Home;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

partial class AccountViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<IAccount> Accounts { get; init; }

    [ObservableProperty]
    private IAccount currentAccount;

    private readonly AccountService _accountService;

    public AccountViewModel(AccountService accountService)
    {
        _accountService = accountService;
        Accounts = accountService.Accounts;
        CurrentAccount = accountService.ActiveAccount;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(this, (r, m) =>
        {
            AccountViewModel vm = r as AccountViewModel;
            vm.CurrentAccount = m.Value;
        });
        OnCurrentAccountChanged(CurrentAccount);
    }

    partial void OnCurrentAccountChanged(IAccount value)
    {
        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = value is not null,
            NextPage = typeof(Views.OOBE.GetStartedPage)
        });
        _accountService.Activate(value);
    }

    [RelayCommand]
    public Task Login(Button parameter) => Task.Run(() =>
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var microsoftAccountDialog = new Views.Common.MicrosoftAccountDialog { XamlRoot = parameter.XamlRoot, };

            var viewmodel = new MicrosoftAccountDialog()
            {
                SetAccountAction = SetAccount,
                ContentDialog = microsoftAccountDialog
            };

            microsoftAccountDialog.DataContext = viewmodel;

            await microsoftAccountDialog.ShowAsync();
        });
    });

    [RelayCommand]
    public Task OfflineLogin(HyperlinkButton parameter) => Task.Run(() =>
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var offlineAccountDialog = new Views.Common.OfflineAccountDialog
            {
                XamlRoot = parameter.XamlRoot,
                DataContext = new OfflineAccountDialog { SetAccountAction = SetAccount }
            };
            await offlineAccountDialog.ShowAsync();
        });
    });

    private void SetAccount(IAccount account) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
#pragma warning disable CS0612 // Type or member is obsolete
        _accountService.AddAccount(account);
#pragma warning restore CS0612 // Type or member is obsolete

        CurrentAccount = account;

        OnPropertyChanged(nameof(Accounts));

        MessageService.ShowSuccess($"Add {account.Type} Account Successfully", $"Welcome back, {account.Name}");
    });
}
using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

partial class AccountViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [BindToSetting(Path = nameof(SettingsService.Accounts))]
    public ObservableCollection<IAccount> Accounts;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentAccount))]
    private IAccount currentAccount;


    public AccountViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();
    }

    partial void OnCurrentAccountChanged(IAccount oldValue, IAccount newValue)
    {
        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = newValue is null,
            NextPage = typeof(Views.OOBE.GetStartedPage)
        });
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
        Accounts.Add(account);
        CurrentAccount = account;

        OnPropertyChanged(nameof(Accounts));

        MessageService.ShowSuccess($"Add {account.Type} Account Successfully", $"Welcome back, {account.Name}");
    });
}
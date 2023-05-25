using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

public partial class AccountViewModel : SettingViewModel
{
    public AccountViewModel() : base()
    {
        OnPropertyChanged(nameof(CanNext));
    }

    [ObservableProperty]
    private bool canNext;

    [ObservableProperty]
    private ObservableCollection<IAccount> accounts;

    [ObservableProperty]
    private IAccount currentAccount;

    protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CanNext))
            CanNext = CurrentAccount != null;

        if (e.PropertyName == nameof(CanNext))
            WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
            {
                CanNext = canNext,
                NextPage = typeof(Views.OOBE.GetStartedPage)
            });
    }
}

public partial class AccountViewModel
{
    [RelayCommand]
    public Task Login(Button parameter) => Task.Run(() =>
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var microsoftAccountDialog = new Views.Dialogs.MicrosoftAccountDialog { XamlRoot = parameter.XamlRoot, };

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
            var offlineAccountDialog = new Views.Dialogs.OfflineAccountDialog
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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Guides;

public partial class Account : SettingViewModel
{
    public Account() : base() 
    {
        OnPropertyChanged("CanNext");
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
                NextPage = typeof(Views.Pages.Guides.GetStarted)
            });
    }
}

public partial class Account
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
            microsoftAccountDialog.Loaded += (_, e) => { viewmodel.Source = new("https://login.live.com/oauth20_authorize.srf?client_id=00000000402b5328&response_type=code&scope=XboxLive.signin%20offline_access&redirect_uri=https://login.live.com/oauth20_desktop.srf&prompt=login"); };

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
    });
}
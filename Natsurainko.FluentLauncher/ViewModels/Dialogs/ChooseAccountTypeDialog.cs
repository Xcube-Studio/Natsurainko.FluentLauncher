using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Views.Pages;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

public partial class ChooseAccountTypeDialog : DialogViewModel
{
    public Action<IAccount> SetAccountAction { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string accountType = "Microsoft";

    protected override bool EnableConfirmButton()
        => !string.IsNullOrEmpty(accountType);

    protected override void OnConfirm(ContentDialog dialog)
    {
        base.OnConfirm(dialog);

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            switch (AccountType)
            {
                case "Microsoft":
                    OnMicrosoft();
                    break;
                case "Yggdrasil":
                    OnYggdrasil();
                    break;
                case "Offline":
                    OnOffline();
                    break;
            }
        });
    }

    protected override void OnCancel(ContentDialog dialog)
    {
        base.OnCancel(dialog);
        MainContainer.ShowMessagesAsync("Cancelled Add Account");
    }

    private async void OnMicrosoft()
    {
        var microsoftAccountDialog = new Views.Dialogs.MicrosoftAccountDialog
        {
            XamlRoot = MainContainer._XamlRoot,
        };

        var viewmodel = new MicrosoftAccountDialog()
        {
            SetAccountAction = SetAccountAction,
            ContentDialog = microsoftAccountDialog
        };

        microsoftAccountDialog.DataContext = viewmodel;
        microsoftAccountDialog.Loaded += (_, e) => { viewmodel.Source = new("https://login.live.com/oauth20_authorize.srf?client_id=00000000402b5328&response_type=code&scope=XboxLive.signin%20offline_access&redirect_uri=https://login.live.com/oauth20_desktop.srf&prompt=login"); };

        await microsoftAccountDialog.ShowAsync();
    }

    private async void OnYggdrasil()
    {
        var yggdrasilAccountDialog = new Views.Dialogs.YggdrasilAccountDialog
        {
            XamlRoot = MainContainer._XamlRoot,
            DataContext = new YggdrasilAccountDialog { SetAccountAction = SetAccountAction }
        };
        await yggdrasilAccountDialog.ShowAsync();
    }

    private async void OnOffline()
    {
        var offlineAccountDialog = new Views.Dialogs.OfflineAccountDialog
        {
            XamlRoot = MainContainer._XamlRoot,
            DataContext = new OfflineAccountDialog { SetAccountAction = SetAccountAction }
        };
        await offlineAccountDialog.ShowAsync();
    }
}
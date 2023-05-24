using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

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
        MessageService.Show("Cancelled Add Account");
    }

    private async void OnMicrosoft()
    {
        if (App.Configuration.UseDeviceFlowAuth)
        {
            var microsoftAccountDialog = new Views.Dialogs.MicrosoftAccountDialog1
            {
                XamlRoot = Views.MainContainer._XamlRoot,
            };

            var viewmodel = new MicrosoftAccountDialog1()
            {
                SetAccountAction = SetAccountAction,
                ContentDialog = microsoftAccountDialog
            };

            microsoftAccountDialog.DataContext = viewmodel;
            microsoftAccountDialog.Loaded += (_, e) =>
            {
                var deviceFlowAuthResult = MicrosoftAuthenticatorExtension.DeviceFlowAuthAsync("0844e754-1d2e-4861-8e2b-18059609badb", res => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    viewmodel.DeviceCode = res.UserCode;
                    viewmodel.LoadingDeviceCode = false;

                    var dataPackage = new DataPackage();
                    dataPackage.SetText(res.UserCode);
                    Clipboard.SetContent(dataPackage);

                    _ = Launcher.LaunchUriAsync(new("https://login.live.com/oauth20_remoteconnect.srf"));
                }), out var cancellationTokenSource);
                viewmodel.CancellationTokenSource = cancellationTokenSource;

                App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    var authResult = await deviceFlowAuthResult;

                    if (authResult.Success)
                        viewmodel.DeviceFlowAuthResult = authResult;
                });

            };

            await microsoftAccountDialog.ShowAsync();
        }
        else
        {
            var microsoftAccountDialog = new Views.Dialogs.MicrosoftAccountDialog
            {
                XamlRoot = Views.MainContainer._XamlRoot,
            };

            var viewmodel = new MicrosoftAccountDialog()
            {
                SetAccountAction = SetAccountAction,
                ContentDialog = microsoftAccountDialog
            };

            microsoftAccountDialog.DataContext = viewmodel;
            
            await microsoftAccountDialog.ShowAsync();
        }
    }

    private async void OnYggdrasil()
    {
        var yggdrasilAccountDialog = new Views.Dialogs.YggdrasilAccountDialog
        {
            XamlRoot = Views.MainContainer._XamlRoot,
            DataContext = new YggdrasilAccountDialog { SetAccountAction = SetAccountAction }
        };
        await yggdrasilAccountDialog.ShowAsync();
    }

    private async void OnOffline()
    {
        var offlineAccountDialog = new Views.Dialogs.OfflineAccountDialog
        {
            XamlRoot = Views.MainContainer._XamlRoot,
            DataContext = new OfflineAccountDialog { SetAccountAction = SetAccountAction }
        };
        await offlineAccountDialog.ShowAsync();
    }
}
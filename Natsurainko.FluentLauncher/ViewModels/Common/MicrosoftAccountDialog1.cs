using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class MicrosoftAccountDialog1 : DialogViewModel
{
    public Action<IAccount> SetAccountAction { get; set; }

    public ContentDialog ContentDialog { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyCommand))]
    private string deviceCode = " ";

    [ObservableProperty]
    private bool loadingDeviceCode = true;

    [ObservableProperty]
    private bool copiedTipIsOpen = false;

    [ObservableProperty]
    private string progress = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private DeviceFlowAuthResult deviceFlowAuthResult;

    [RelayCommand]
    public async Task Copy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(DeviceCode);
        Clipboard.SetContent(dataPackage);

        App.MainWindow.DispatcherQueue.TryEnqueue(() => CopiedTipIsOpen = true);
        await Task.Delay(2000);
        App.MainWindow.DispatcherQueue.TryEnqueue(() => CopiedTipIsOpen = false);
    }

    protected bool EnableCopyButton() => DeviceCode != null;

    protected override bool EnableConfirmButton() => DeviceFlowAuthResult != null;

    protected override void OnCancel(ContentDialog dialog)
    {
        base.OnCancel(dialog);

        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }

    protected override void OnConfirm(ContentDialog dialog)
    {
        try
        {
            var authenticator = new MicrosoftAuthenticator(deviceFlowAuthResult.OAuth20TokenResponse, "0844e754-1d2e-4861-8e2b-18059609badb", "https://login.live.com/oauth20_desktop.srf");
            authenticator.ProgressChanged += (_, e) => ContentDialog.DispatcherQueue.TryEnqueue(() => Progress = e.Item2);

            SetAccountAction(authenticator.AuthenticateAsync().GetAwaiter().GetResult());
        }
        catch (Exception ex)
        {
            MessageService.ShowException(ex, "Failed to Add Microsoft Account");
        }

        CancellationTokenSource.Dispose();
        base.OnConfirm(dialog);
    }
}

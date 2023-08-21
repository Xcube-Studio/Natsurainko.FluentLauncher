using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.DefaultComponents.Authenticate;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class DeviceFlowMicrosoftAuthViewModel : WizardViewModelBase
{
    public override bool CanNext => DeviceFlowAuthResult != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private DeviceFlowResponse deviceFlowAuthResult;

    [ObservableProperty]
    private string deviceCode;

    [ObservableProperty]
    private bool loading = true;

    private bool Unloaded = false;

    private readonly AuthenticationService _authenticationService;

    internal CancellationTokenSource CancellationTokenSource;
    internal Task<DeviceFlowResponse> DeviceFlowProcess;

    public DeviceFlowMicrosoftAuthViewModel()
    {
        XamlPageType = typeof(DeviceFlowMicrosoftAuthPage);

        _authenticationService = App.GetService<AuthenticationService>();

        CreateDeviceFlowProcess();
    }

    [RelayCommand]
    public Task RefreshCode() => Task.Run(() =>
    {
        App.DispatcherQueue.SynchronousTryEnqueue(() => Loading = true);

        CancellationTokenSource.Cancel();
        if (DeviceFlowProcess.Status == TaskStatus.Running)
            DeviceFlowProcess.Wait();

        CreateDeviceFlowProcess();
    });

    [RelayCommand]
    public void CopyCode() => Copy();

    [RelayCommand]
    public void UnloadEvent(object args)
    {
        CancellationTokenSource?.Cancel();
        Unloaded = true;
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        ConfirmProfileViewModel confirmProfileViewModel = default;

        confirmProfileViewModel = new ConfirmProfileViewModel(() => new Account[]
        {
            _authenticationService.AuthenticateMicrosoft(DeviceFlowAuthResult,
                progress => App.DispatcherQueue.TryEnqueue(() => confirmProfileViewModel.LoadingProgressText = progress))
        });

        return confirmProfileViewModel;
    }

    private void CreateDeviceFlowProcess()
    {
        CancellationTokenSource?.Dispose();

        DeviceFlowProcess = DefaultMicrosoftAuthenticator.DeviceFlowAuthAsync(AuthenticationService.ClientId,
            res => App.DispatcherQueue.TryEnqueue(() =>
            {
                DeviceCode = res.UserCode;
                Loading = false;

                Copy();
                _ = Launcher.LaunchUriAsync(new("https://login.live.com/oauth20_remoteconnect.srf"));
            }),
            out var cancellationTokenSource);
        CancellationTokenSource = cancellationTokenSource;

        DeviceFlowProcess.ContinueWith(task =>
        {
            if (task.IsFaulted || !task.Result.Success || (CancellationTokenSource.IsCancellationRequested && !task.Result.Success))
            {
                App.DispatcherQueue.SynchronousTryEnqueue(() =>
                {
                    DeviceCode = "Failed";
                    Loading = false;
                });

                return;
            }

            if (!Unloaded && task.Result.Success)
                App.DispatcherQueue.SynchronousTryEnqueue(() => DeviceFlowAuthResult = task.Result);
        });
    }

    private void Copy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(DeviceCode);
        Clipboard.SetContent(dataPackage);
    }
}

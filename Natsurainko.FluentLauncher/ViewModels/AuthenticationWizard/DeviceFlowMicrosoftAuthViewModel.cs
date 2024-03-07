using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Authentication.Microsoft;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class DeviceFlowMicrosoftAuthViewModel : WizardViewModelBase
{
    public override bool CanNext => _canNext;

    private bool _canNext = false;

    //[ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(CanNext))]
    //private DeviceFlowResponse deviceFlowAuthResult;

    [ObservableProperty]
    private string deviceCode;

    [ObservableProperty]
    private bool loading = true;

    private bool Unloaded = false;

    private readonly AuthenticationService _authenticationService;

    internal CancellationTokenSource CancellationTokenSource;
    internal Task<MicrosoftAccount> DeviceFlowProcess;

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
        ConfirmProfileViewModel confirmProfileViewModel = new ConfirmProfileViewModel(() => new Account[]
        {
            DeviceFlowProcess.GetAwaiter().GetResult()
        });

        return confirmProfileViewModel;
    }

    private void CreateDeviceFlowProcess()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();

        // Display user code when received and launch browser
        var receiveUserCodeAction = (DeviceCodeResponse res) =>
        {
            App.DispatcherQueue.TryEnqueue(async () =>
            {
                DeviceCode = res.UserCode!;
                Loading = false;

                Copy();
                await Launcher.LaunchUriAsync(new("https://login.live.com/oauth20_remoteconnect.srf"));
            });
        };

        var progress = new Progress<MicrosoftAccountAuthenticationProgress>((p) =>
        {
            // User has entered the device code and msaOAuth is completed successfully
            if (!Unloaded && p == MicrosoftAccountAuthenticationProgress.AuthenticatingWithXboxLive)
            {
                App.DispatcherQueue.TryEnqueue(() =>
                {
                    _canNext = true;
                    OnPropertyChanged(nameof(CanNext)); // Enable the next button to allow proceeding to the confirmation page
                });
            }
        });

        try
        {
            DeviceFlowProcess = _authenticationService
                .LoginMicrosoft(receiveUserCodeAction, CancellationTokenSource.Token, progress);
        }
        catch (MicrosoftAccountAuthenticationException)
        {
            // handle failed authentication
            App.DispatcherQueue.SynchronousTryEnqueue(() =>
            {
                DeviceCode = "Failed";
                Loading = false;
            });
        }
        catch (OperationCanceledException) { }
    }

    private void Copy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(DeviceCode);
        Clipboard.SetContent(dataPackage);
    }
}

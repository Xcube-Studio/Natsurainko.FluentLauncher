using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Authentication;
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
    private string? deviceCode;

    [ObservableProperty]
    private bool loading = true;

    private bool Unloaded = false;

    private readonly AuthenticationService _authService;

    // Started in the constructor
    internal CancellationTokenSource CancellationTokenSource = null!;
    internal Task<OAuth2Tokens> DeviceFlowProcess = null!;

    public DeviceFlowMicrosoftAuthViewModel()
    {
        XamlPageType = typeof(DeviceFlowMicrosoftAuthPage);

        _authService = App.GetService<AuthenticationService>();

        _ = CreateDeviceFlowProcessAsync();
    }

    [RelayCommand]
    public async Task RefreshCode()
    {
        Loading = true;

        CancellationTokenSource.Cancel();
        try
        {
            await DeviceFlowProcess; // Wait for polling to stop after cancellation requested
        }
        catch (OperationCanceledException) { }

        _ = CreateDeviceFlowProcessAsync(); // Start a new device flow process
    }

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
            _authService.LoginMicrosoftAsync(DeviceFlowProcess.GetAwaiter().GetResult()).GetAwaiter().GetResult()
        });

        return confirmProfileViewModel;
    }

    private async Task CreateDeviceFlowProcessAsync()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();

        // Display user code when received and launch browser
        var receiveUserCodeAction = (OAuth2DeviceCodeResponse response) =>
        {
            App.DispatcherQueue.TryEnqueue(async () =>
            {
                DeviceCode = response.UserCode!; // TODO: Make UserCOde required
                Loading = false;

                Copy();
                await Launcher.LaunchUriAsync(new("https://login.live.com/oauth20_remoteconnect.srf"));
            });
        };

        try
        {
            DeviceFlowProcess = _authService.AuthMsaFromDeviceFlowAsync(receiveUserCodeAction, CancellationTokenSource.Token);
            await DeviceFlowProcess;
        }
        catch (MicrosoftAuthenticationException)
        {
            // handle failed authentication
            App.DispatcherQueue.SynchronousTryEnqueue(() =>
            {
                DeviceCode = "Failed";
                Loading = false;
            });
        }
        catch (OperationCanceledException) { }

        // User has entered the device code and msaOAuth is completed successfully
        if (!Unloaded && DeviceFlowProcess.IsCompletedSuccessfully)
        {
            _canNext = true;
            OnPropertyChanged(nameof(CanNext)); // Enable the next button to allow proceeding to the next page
        }
    }

    private void Copy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(DeviceCode);
        Clipboard.SetContent(dataPackage);
    }
}

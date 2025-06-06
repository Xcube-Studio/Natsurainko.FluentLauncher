﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class DeviceFlowMicrosoftAuthViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authService;

    public override bool CanNext => _canNext;

    private bool _canNext = false;

    //[ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(CanNext))]
    //private DeviceFlowResponse deviceFlowAuthResult;

    [ObservableProperty]
    public partial string? DeviceCode { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; } = true;

    private bool Unloaded = false;

    internal Task<OAuth2Tokens> DeviceFlowProcess = null!;

    public DeviceFlowMicrosoftAuthViewModel(AuthenticationService authService)
    {
        _authService = authService;
        XamlPageType = typeof(DeviceFlowMicrosoftAuthPage);

        // Safe to fire and forget as all exceptions are handled
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
        // When refresh is clicked, failed device flow auth will throw an exception here.
        // It should be handled already in CreateDeviceFlowProcessAsync
        catch (MicrosoftAuthenticationException) { }
        catch (OperationCanceledException) { }

        // Safe to fire and forget as all exceptions are handled
        _ = CreateDeviceFlowProcessAsync(); // Start a new device flow process
    }

    [RelayCommand]
    public void CopyCode() => ClipboardHepler.SetText(DeviceCode!);

    [RelayCommand]
    public void UnloadEvent(object args)
    {
        CancellationTokenSource?.Cancel();
        Unloaded = true;
    }

    public override WizardViewModelBase GetNextViewModel()
        => new ConfirmProfileViewModel(async cancellationToken => [await _authService.LoginMicrosoftAsync(await DeviceFlowProcess)]);

    private async Task CreateDeviceFlowProcessAsync()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();

        // Display user code when received and launch browser
        var receiveUserCodeAction = (OAuth2DeviceCodeResponse response) =>
        {
            App.DispatcherQueue.EnqueueAsync(async () =>
            {
                DeviceCode = response.UserCode;
                Loading = false;

                CopyCode();
                await Launcher.LaunchUriAsync(new("https://login.live.com/oauth20_remoteconnect.srf"));
            });
        };

        try
        {
            DeviceFlowProcess = _authService.AuthMsaFromDeviceFlowAsync(receiveUserCodeAction, CancellationTokenSource.Token);
            await DeviceFlowProcess;
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            // handle all exceptions
            DeviceCode = "Failed";
            Loading = false;
        }

        // User has entered the device code and msaOAuth is completed successfully
        if (!Unloaded && DeviceFlowProcess.IsCompletedSuccessfully)
        {
            _canNext = true;
            OnPropertyChanged(nameof(CanNext)); // Enable the next button to allow proceeding to the next page
        }
    }
}

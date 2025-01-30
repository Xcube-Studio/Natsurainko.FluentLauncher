using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using System;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ChooseMicrosoftAuthMethodViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authService;
    public override bool CanNext => true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial MicrosoftAuthMethod SelectedMicrosoftAuthMethod { get; set; } = MicrosoftAuthMethod.BuiltInBrowser;

    public List<MicrosoftAuthMethod> MicrosoftAuthMethods { get; } = [MicrosoftAuthMethod.BuiltInBrowser, MicrosoftAuthMethod.DeviceFlowCode];

    public ChooseMicrosoftAuthMethodViewModel(AuthenticationService authService)
    {
        _authService = authService;
        XamlPageType = typeof(ChooseMicrosoftAuthMethodPage);
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        return SelectedMicrosoftAuthMethod switch
        {
            MicrosoftAuthMethod.BuiltInBrowser => new BrowserMicrosoftAuthViewModel(_authService),
            MicrosoftAuthMethod.DeviceFlowCode => new DeviceFlowMicrosoftAuthViewModel(_authService),
            _ => throw new InvalidOperationException()
        };
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ChooseMicrosoftAuthMethodViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authService;
    public override bool CanNext => SelectedMicrosoftAuthMethod != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private MicrosoftAuthMethod? selectedMicrosoftAuthMethod = MicrosoftAuthMethod.BuiltInBrowser;

    public ChooseMicrosoftAuthMethodViewModel(AuthenticationService authService)
    {
        _authService = authService;
        XamlPageType = typeof(ChooseMicrosoftAuthMethodPage);
    }

    [RelayCommand]
    public void Checked(int index) => SelectedMicrosoftAuthMethod = (MicrosoftAuthMethod)index;

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

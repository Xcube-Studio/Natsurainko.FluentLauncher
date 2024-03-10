using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ChooseMicrosoftAuthMethodViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authService;
    public override bool CanNext => SelectedMicrosoftAuthMethod != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private MicrosoftAuthMethod? selectedMicrosoftAuthMethod;

    public ChooseMicrosoftAuthMethodViewModel(AuthenticationService authService)
    {
        _authService = authService;
        XamlPageType = typeof(ChooseMicrosoftAuthMethodPage);
    }

    [RelayCommand]
    public void Checked(string method)
    {
        SelectedMicrosoftAuthMethod = method switch
        {
            "BuiltInBrowser" => MicrosoftAuthMethod.BuiltInBrowser,
            "DeviceFlowCode" => MicrosoftAuthMethod.DeviceFlowCode,
            _ => null
        };
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        return SelectedMicrosoftAuthMethod switch
        {
            MicrosoftAuthMethod.BuiltInBrowser => new BrowserMicrosoftAuthViewModel(),
            MicrosoftAuthMethod.DeviceFlowCode => new DeviceFlowMicrosoftAuthViewModel(_authService),
            _ => null
        };
    }
}

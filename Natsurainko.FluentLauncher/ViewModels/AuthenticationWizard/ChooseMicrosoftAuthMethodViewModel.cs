using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ChooseMicrosoftAuthMethodViewModel : WizardViewModelBase
{
    public override bool CanNext => SelectedMicrosoftAuthMethod != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private MicrosoftAuthMethod? selectedMicrosoftAuthMethod;

    public ChooseMicrosoftAuthMethodViewModel()
    {
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
            MicrosoftAuthMethod.DeviceFlowCode => new DeviceFlowMicrosoftAuthViewModel(),
            _ => null
        };
    }
}

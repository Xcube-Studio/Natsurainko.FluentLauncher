using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.CoreInstallWizard;

namespace Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;

internal partial class AdditionalOptionsViewModel : WizardViewModelBase
{
    public override bool CanNext => true;

    public override bool CanPrevious => true;

    public readonly CoreInstallationInfo _coreInstallationInfo;

    public AdditionalOptionsViewModel(CoreInstallationInfo coreInstallationInfo) 
    {
        XamlPageType = typeof(AdditionalOptionsPage);

        _coreInstallationInfo = coreInstallationInfo;
    }
}

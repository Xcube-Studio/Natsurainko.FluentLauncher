using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ChooseAccountTypePage : Page
{
    // TODO: Fix crash due to DataContext being null when navigating back in the wizard

    // Investigation: Two instances of this page are referencing the same VM after navigating back.
    // Changes on the page being displayed cause changes in the VM, which triggers an update on the page unloaded.
    // Radio buttons on the page unloaded are updated, but its DataContext is null, resulting in a crash.

    ChooseAccountTypeViewModel VM => (ChooseAccountTypeViewModel)DataContext;

    public ChooseAccountTypePage()
    {
        InitializeComponent();
    }
}

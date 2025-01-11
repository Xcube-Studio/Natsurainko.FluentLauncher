using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ChooseAccountTypePage : Page
{
    // Note: The app will crash if two instances of this page are referencing the same VM after navigating back.
    // Changes on the page being displayed cause changes in the VM, which triggers an update on the page unloaded.
    // Radio buttons on the page unloaded are updated, but its DataContext is null, resulting in a crash.

    ChooseAccountTypeViewModel VM => (ChooseAccountTypeViewModel)DataContext;

    public ChooseAccountTypePage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DataContext = new ChooseAccountTypeViewModel(null!);
    }
}

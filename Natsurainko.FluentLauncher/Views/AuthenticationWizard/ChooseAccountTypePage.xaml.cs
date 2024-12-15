using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ChooseAccountTypePage : Page
{
    // TODO: Fix crash due to DataContext being null when navigating back in the wizard
    ChooseAccountTypeViewModel VM => (ChooseAccountTypeViewModel)DataContext;

    public ChooseAccountTypePage()
    {
        InitializeComponent();
    }
}

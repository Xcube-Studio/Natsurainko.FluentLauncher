using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ConfirmProfilePage : Page
{
    ConfirmProfileViewModel VM => (ConfirmProfileViewModel)DataContext;

    public ConfirmProfilePage()
    {
        InitializeComponent();
    }
}

using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class EnterYggdrasilProfilePage : Page
{
    EnterYggdrasilProfileViewModel VM => (EnterYggdrasilProfileViewModel)DataContext;

    public EnterYggdrasilProfilePage()
    {
        InitializeComponent();
    }
}

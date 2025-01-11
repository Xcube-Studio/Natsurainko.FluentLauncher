using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class EnterOfflineProfilePage : Page
{
    EnterOfflineProfileViewModel VM => (EnterOfflineProfileViewModel)DataContext;

    public EnterOfflineProfilePage()
    {
        InitializeComponent();
    }
}

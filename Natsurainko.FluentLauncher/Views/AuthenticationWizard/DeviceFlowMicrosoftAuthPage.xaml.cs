using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class DeviceFlowMicrosoftAuthPage : Page
{
    DeviceFlowMicrosoftAuthViewModel VM => (DeviceFlowMicrosoftAuthViewModel)DataContext;

    public DeviceFlowMicrosoftAuthPage()
    {
        InitializeComponent();
    }
}

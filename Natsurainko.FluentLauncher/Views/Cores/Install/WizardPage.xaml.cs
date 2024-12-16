using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Install;

namespace Natsurainko.FluentLauncher.Views.Cores.Install;

public sealed partial class WizardPage : Page
{
    WizardViewModel VM => (WizardViewModel)DataContext;

    public WizardPage()
    {
        InitializeComponent();
    }
}

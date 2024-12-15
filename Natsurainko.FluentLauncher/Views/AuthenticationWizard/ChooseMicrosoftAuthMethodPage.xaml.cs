using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ChooseMicrosoftAuthMethodPage : Page
{
    ChooseMicrosoftAuthMethodViewModel VM => (ChooseMicrosoftAuthMethodViewModel)DataContext;

    public ChooseMicrosoftAuthMethodPage()
    {
        InitializeComponent();
    }
}

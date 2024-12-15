using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class BrowserMicrosoftAuthPage : Page
{
    BrowserMicrosoftAuthViewModel VM => (BrowserMicrosoftAuthViewModel)DataContext;

    public BrowserMicrosoftAuthPage()
    {
        InitializeComponent();
    }
}

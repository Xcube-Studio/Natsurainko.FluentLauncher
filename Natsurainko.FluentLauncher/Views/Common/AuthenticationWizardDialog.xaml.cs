using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class AuthenticationWizardDialog : ContentDialog
{
    AuthenticationWizardDialogViewModel VM => (AuthenticationWizardDialogViewModel)DataContext;

    public AuthenticationWizardDialog()
    {
        InitializeComponent();
    }
}

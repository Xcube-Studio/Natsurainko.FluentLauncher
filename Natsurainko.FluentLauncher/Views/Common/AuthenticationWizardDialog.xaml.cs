using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class AuthenticationWizardDialog : ContentDialog
{
    public AuthenticationWizardDialog()
    {
        this.XamlRoot = MainWindow.XamlRoot;

        this.InitializeComponent();
        DataContext = new AuthenticationWizardDialogViewModel(
            App.GetService<AccountService>(),
            App.GetService<NotificationService>(),
            App.GetService<AuthenticationService>());
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class AuthenticationWizardDialog : ContentDialog
{
    AuthenticationWizardDialogViewModel VM => (AuthenticationWizardDialogViewModel)DataContext;

    public AuthenticationWizardDialog()
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}

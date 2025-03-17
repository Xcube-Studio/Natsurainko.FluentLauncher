using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class AuthenticationWizardDialog : ContentDialog
{
    AuthenticationWizardDialogViewModel VM => (AuthenticationWizardDialogViewModel)DataContext;

    public AuthenticationWizardDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }
}

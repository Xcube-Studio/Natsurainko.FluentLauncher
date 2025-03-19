using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class AuthenticateDialog : ContentDialog
{
    AuthenticateDialogViewModel VM => (AuthenticateDialogViewModel)DataContext;

    public AuthenticateDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }
}

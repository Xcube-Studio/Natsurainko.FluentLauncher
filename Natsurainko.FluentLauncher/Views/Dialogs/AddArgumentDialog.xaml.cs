using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

internal sealed partial class AddArgumentDialog : ContentDialog
{
    AddArgumentDialogViewModel VM => (AddArgumentDialogViewModel)DataContext;

    public AddArgumentDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }
}

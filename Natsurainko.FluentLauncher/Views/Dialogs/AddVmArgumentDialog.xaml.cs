using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

internal sealed partial class AddVmArgumentDialog : ContentDialog
{
    AddVmArgumentDialogViewModel VM => (AddVmArgumentDialogViewModel)DataContext;

    public AddVmArgumentDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }
}

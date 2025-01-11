using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

internal sealed partial class AddVmArgumentDialog : ContentDialog
{
    AddVmArgumentDialogViewModel VM => (AddVmArgumentDialogViewModel)DataContext;

    public AddVmArgumentDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }
}

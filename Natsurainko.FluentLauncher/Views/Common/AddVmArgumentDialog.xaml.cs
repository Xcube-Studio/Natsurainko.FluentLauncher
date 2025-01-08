using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Views.Common;

internal sealed partial class AddVmArgumentDialog : ContentDialog
{
    public AddVmArgumentDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}

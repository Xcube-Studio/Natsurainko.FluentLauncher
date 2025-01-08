using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DeleteInstanceDialog : ContentDialog
{
    public DeleteInstanceDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}

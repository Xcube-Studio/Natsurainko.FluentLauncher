using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Views.Common;

internal sealed partial class SwitchAccountDialog : ContentDialog
{
    public SwitchAccountDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }

    private void ContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ListView.ItemsSource = null; // Unload Binding to AccountService.Accounts
    }
}

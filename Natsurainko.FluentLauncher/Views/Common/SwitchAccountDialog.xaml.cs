using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

internal sealed partial class SwitchAccountDialog : ContentDialog
{
    SwitchAccountDialogViewModel VM => (SwitchAccountDialogViewModel)DataContext;

    public SwitchAccountDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }

    private void ContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ListView.ItemsSource = null; // Unload Binding to AccountService.Accounts
    }
}

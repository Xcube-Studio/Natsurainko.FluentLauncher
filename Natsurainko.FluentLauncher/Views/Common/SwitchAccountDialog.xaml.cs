using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

internal sealed partial class SwitchAccountDialog : ContentDialog
{
    SwitchAccountDialogViewModel VM => (SwitchAccountDialogViewModel)DataContext;

    public SwitchAccountDialog()
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }

    private void ContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ListView.ItemsSource = null; // Unload Binding to AccountService.Accounts
    }
}

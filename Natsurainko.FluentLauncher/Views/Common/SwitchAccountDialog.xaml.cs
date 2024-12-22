using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class SwitchAccountDialog : ContentDialog
{
    public SwitchAccountDialog()
    {
        InitializeComponent();
    }

    private void ContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ListView.ItemsSource = null; // Unload Binding to AccountService.Accounts
    }
}

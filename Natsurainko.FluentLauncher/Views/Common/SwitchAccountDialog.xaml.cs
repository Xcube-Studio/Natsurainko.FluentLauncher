using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class SwitchAccountDialog : ContentDialog
{
    SwitchAccountDialogViewModel VM => (SwitchAccountDialogViewModel)DataContext;

    public SwitchAccountDialog()
    {
        XamlRoot = MainWindow.XamlRoot;
        InitializeComponent();
    }

    private void ContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ListView.ItemsSource = null; // Unload Binding to AccountService.Accounts
    }
}

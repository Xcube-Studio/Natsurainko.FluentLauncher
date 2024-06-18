using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class AccountPage : Page
{
    public AccountPage()
    {
        InitializeComponent();
    }

    private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var grid = (Grid)sender;
        var button = (Button)grid.FindName("DeleteButton");
        button.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
    }

    private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var grid = (Grid)sender;
        var button = (Button)grid.FindName("DeleteButton");
        button.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = this.DataContext as OOBEViewModel;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(vm!, (r, m) =>
        {
            OOBEViewModel vm = (r as OOBEViewModel)!;

            vm.processingActiveAccountChangedMessage = true;
            vm.ActiveAccount = m.Value;
            vm.processingActiveAccountChangedMessage = false;
        });

        void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<ActiveAccountChangedMessage>(vm!);

            ListView.ItemsSource = null; // Unload Binding to AccountService.Accounts
        }

        this.Unloaded += Page_Unloaded;
    }
}

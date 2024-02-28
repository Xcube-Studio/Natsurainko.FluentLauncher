using Microsoft.UI.Xaml.Controls;

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
}

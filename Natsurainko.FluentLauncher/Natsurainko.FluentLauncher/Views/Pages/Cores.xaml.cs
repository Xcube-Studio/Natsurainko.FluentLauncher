using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Pages;

public sealed partial class Cores : Page
{
    public Cores()
    {
        InitializeComponent();
    }

    private void Border_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        => ((Grid)((Border)sender).FindName("ControlPanel")).Visibility = Visibility.Visible;

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        => ((Grid)((Border)sender).FindName("ControlPanel")).Visibility = Visibility.Collapsed;
}

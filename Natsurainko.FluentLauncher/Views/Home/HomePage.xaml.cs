using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        LaunchButton.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }
}

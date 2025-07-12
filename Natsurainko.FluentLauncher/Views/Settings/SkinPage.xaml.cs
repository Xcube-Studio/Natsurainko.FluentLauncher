using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class SkinPage : Page, IBreadcrumbBarAware
{
    public string Route => "Skin";

    SkinViewModel VM => (SkinViewModel)DataContext;

    public SkinPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Viewport.BackgroundColor = ActualTheme switch 
        {
            ElementTheme.Dark => Colors.Black,
            ElementTheme.Light => Colors.White,
            _ => Colors.Transparent
        };
        Viewport.Background = new SolidColorBrush(Viewport.BackgroundColor);

        VM.LoadingWidth = Viewport.ActualWidth;
        await VM.LoadTextureProfile();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        Viewport.Dispose();
        VM.Dispose();
    }
}

using FluentLauncher.Infra.UI.Navigation;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SharpDX;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class SkinPage : Page, IBreadcrumbBarAware
{
    public string Route => "Skin";

    public SkinPage()
    {
        this.InitializeComponent();

        this.Loaded += Page_Loaded;
        this.Unloaded += Page_Unloaded;
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Viewport3DX.EffectsManager = new DefaultEffectsManager();
        Viewport3DX.Camera = new PerspectiveCamera()
        {
            Position = new Vector3(130, 95, 265),
            LookDirection = new Vector3(-130, 0, -265),
            NearPlaneDistance = 0.1
        };

        var brush = this.Resources["ContentDialogBackground"] as SolidColorBrush;
        Viewport3DX.Background = brush;
        Viewport3DX.BackgroundColor = brush.Color;
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Viewport3DX.Dispose();
    }
}

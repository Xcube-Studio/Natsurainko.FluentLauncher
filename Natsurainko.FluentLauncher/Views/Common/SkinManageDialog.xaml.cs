using HelixToolkit.SharpDX.Core;
using HelixToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SharpDX;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class SkinManageDialog : ContentDialog
{
    public SkinManageDialog()
    {
        this.InitializeComponent();

        this.Loaded += SkinManageDialog_Loaded;
        this.Unloaded += SkinManageDialog_Unloaded;
    }

    private void SkinManageDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

    private void SkinManageDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Viewport3DX.Dispose();
    }
}

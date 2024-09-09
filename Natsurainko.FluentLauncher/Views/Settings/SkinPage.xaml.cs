using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Settings;
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

        var brush = (SolidColorBrush)Resources["ContentDialogBackground"];
        Viewport3DX.Background = brush;
        Viewport3DX.BackgroundColor = brush.Color;

        WeakReferenceMessenger.Default.Register<AccountSkinCacheUpdatedMessage>(this, (r, m) =>
        {
            var vm = (SkinViewModel)DataContext;

            if (m.Value.Type == vm!.ActiveAccount.Type && m.Value.Uuid == vm!.ActiveAccount.Uuid)
                _ = vm.LoadModel();
        });
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Viewport3DX.Dispose();

        WeakReferenceMessenger.Default.Unregister<AccountSkinCacheUpdatedMessage>(this);
    }
}

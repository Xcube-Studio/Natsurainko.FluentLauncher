using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Settings;
using SharpDX;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class SkinPage : Page, IBreadcrumbBarAware
{
    public string Route => "Skin";

    SkinViewModel VM => (SkinViewModel)DataContext;

    public SkinPage()
    {
        InitializeComponent();

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Viewport3DX.EffectsManager = new DefaultEffectsManager();
        Viewport3DX.Camera = new PerspectiveCamera()
        {
            Position = new Vector3(130, 95, 265),
            LookDirection = new Vector3(-130, 0, -265),
            NearPlaneDistance = 0.1
        };

        Viewport3DX.BackgroundColor = ActualTheme switch 
        {
            ElementTheme.Dark => Colors.Black,
            ElementTheme.Light => Colors.White,
            _ => Colors.Transparent
        };
        Viewport3DX.Background = new SolidColorBrush(Viewport3DX.BackgroundColor);

        WeakReferenceMessenger.Default.Register<AccountSkinCacheUpdatedMessage>(this, (r, m) =>
        {
            var vm = (SkinViewModel)DataContext;

            if (m.Value.Type == vm!.ActiveAccount.Type && m.Value.Uuid == vm!.ActiveAccount.Uuid)
                _ = vm.LoadModel();
        });
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        Viewport3DX.Dispose();

        WeakReferenceMessenger.Default.Unregister<AccountSkinCacheUpdatedMessage>(this);
    }
}

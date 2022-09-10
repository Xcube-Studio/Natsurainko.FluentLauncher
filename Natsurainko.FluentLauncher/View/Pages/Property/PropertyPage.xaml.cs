using Natsurainko.FluentLauncher.Class.ViewData;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Property;

public sealed partial class PropertyPage : Page
{
    public GameCoreViewData GameCore { get; set; }

    public PropertyPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        this.GameCore = e.Parameter as GameCoreViewData;
        Core.DataContext = GameCore;
    }

    private void NavigationView_ItemInvoked(Muxc.NavigationView sender, Muxc.NavigationViewItemInvokedEventArgs args)
    {
        if (((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag != null)
            contentFrame.Navigate(Type.GetType($"Natsurainko.FluentLauncher.View.Pages.Property.{((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag}"), GameCore);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
        => contentFrame.Navigate(typeof(PropertyDetailsPage), GameCore);
}

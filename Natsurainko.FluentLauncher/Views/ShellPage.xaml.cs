using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.Linq;
using Windows.Graphics;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ShellPage : Page
{
    public static XamlRoot _XamlRoot { get; private set; }
    public static Frame ContentFrame { get; private set; }

    private readonly SettingsService _settings = App.GetService<SettingsService>();

    public ShellPage()
    {
        /*
        this.Resources.Add("NavigationViewContentBackground", new SolidColorBrush(Colors.Transparent));
        this.Resources.Add("NavigationViewPaneContentGridMargin", new Thickness(-1, 0, -1, 0));
        this.Resources.Add("NavigationViewContentGridCornerRadius", new CornerRadius(0));
        */
        InitializeComponent();

        ContentFrame = contentFrame;
    }

    private void UpdateAppTitleMargin(NavigationView sender)
    {
        AppTitle.TranslationTransition = new Vector3Transition();
        AppTitle.Translation = ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                 sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                 ? new System.Numerics.Vector3(8, 0, 0)
                 : new System.Numerics.Vector3(28, 0, 0);
    }

    private void NavigationViewControl_PaneClosing(NavigationView sender, object _)
        => UpdateAppTitleMargin(sender);

    private void NavigationViewControl_PaneOpening(NavigationView sender, object _)
        => UpdateAppTitleMargin(sender);

    private void NavigationViewControl_ItemInvoked(NavigationView _, NavigationViewItemInvokedEventArgs args)
        => contentFrame.Navigate(Type.GetType(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()));

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        => contentFrame.GoBack();

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        var currMargin = AppTitleBar.Margin;
        AppTitleBar.Margin = new Thickness(sender.DisplayMode == NavigationViewDisplayMode.Minimal ? (sender.CompactPaneLength * 2) : sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
        UpdateAppTitleMargin(sender);
        RefreshDragArea();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _XamlRoot = XamlRoot;

        App.MainWindow.SetTitleBar(AppTitleBar);
        contentFrame.Navigate(typeof(Home.HomePage));

        RefreshDragArea();
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RefreshDragArea();

        _settings.AppWindowWidth = App.MainWindow.Width;
        _settings.AppWindowHeight = App.MainWindow.Height;
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (NavigationViewItem item in NavigationViewControl.MenuItems.Union(NavigationViewControl.FooterMenuItems).Cast<NavigationViewItem>())
        {
            if (Type.GetType((string)item.Tag) == e.SourcePageType)
            {
                NavigationViewControl.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
        }
    }

    private void RefreshDragArea()
    {
        var scaleAdjustment = XamlRoot.RasterizationScale;

        var x = (int)(AppTitleBar.Margin.Left * scaleAdjustment);
        var y = 0;
        var width = (int)(AppTitleBar.ActualWidth * scaleAdjustment);
        var height = (int)(48 * scaleAdjustment);

        var dragRect = new RectInt32(x, y, width, height);
        App.MainWindow.AppWindow.TitleBar.SetDragRectangles(new[] { dragRect });
    }

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        /*
        var PaneContentGrid = FindChildByName(NavigationViewControl, "PaneContentGrid");

        if (PaneContentGrid != null)
        {
            var acrylic = new AcrylicBrush
            {
                TintOpacity = 0.25,
                TintLuminosityOpacity = 0.25,
                FallbackColor = ActualTheme == ElementTheme.Dark ? Colors.Black : Colors.White,
                TintColor = ActualTheme == ElementTheme.Dark ? Colors.Black : Colors.White
            };

            PaneContentGrid.SetValue(Grid.BackgroundProperty, acrylic);
        }*/
    }

    private void Page_ActualThemeChanged(FrameworkElement sender, object args)
    {
        /*
        var PaneContentGrid = FindChildByName(NavigationViewControl, "PaneContentGrid");

        if (PaneContentGrid != null)
        {
            var acrylic = new AcrylicBrush
            {
                TintOpacity = 0.25,
                TintLuminosityOpacity = 0.25,
                FallbackColor = ActualTheme == ElementTheme.Dark ? Colors.Black : Colors.White,
                TintColor = ActualTheme == ElementTheme.Dark ? Colors.Black : Colors.White
            };

            PaneContentGrid.SetValue(Grid.BackgroundProperty, acrylic);
        }*/
    }

    /*
    public static DependencyObject FindChildByName(DependencyObject parant, string ControlName)
    {
        int count = VisualTreeHelper.GetChildrenCount(parant);

        for (int i = 0; i < count; i++)
        {
            var MyChild = VisualTreeHelper.GetChild(parant, i);
            if (MyChild is FrameworkElement && ((FrameworkElement)MyChild).Name == ControlName)
                return MyChild;

            var FindResult = FindChildByName(MyChild, ControlName);
            if (FindResult != null)
                return FindResult;
        }

        return null;
    }*/

}

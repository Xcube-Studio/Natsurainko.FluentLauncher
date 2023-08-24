using CommunityToolkit.WinUI.UI.Media.Pipelines;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using System;
using System.Linq;
using Windows.Graphics;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ShellPage : Page
{
    public static XamlRoot _XamlRoot { get; private set; }
    public static Frame ContentFrame { get; private set; }

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly AppearanceService _appearanceService = App.GetService<AppearanceService>();

    public ShellPage()
    {
        _appearanceService.ApplyBackgroundBeforePageInit(this);
        InitializeComponent();

        ContentFrame = contentFrame;
        _appearanceService.ApplyBackgroundAfterPageInit(this);
        _appearanceService.RegisterNavigationView(NavigationViewControl);
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
    {
        if (((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()
            .Equals("Natsurainko.FluentLauncher.Views.Home.HomePage"))
        {
            contentFrame.Navigate(App.GetService<AppearanceService>().HomePageType);
            return;
        }

        contentFrame.Navigate(Type.GetType(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()));
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        => contentFrame.GoBack();

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        var currMargin = AppTitleBar.Margin;
        AppTitleBar.Margin = new Thickness(sender.DisplayMode == NavigationViewDisplayMode.Minimal ? (sender.CompactPaneLength * 2) : sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
        UpdateAppTitleMargin(sender);
        RefreshDragArea();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _XamlRoot = XamlRoot;

        App.MainWindow.SetTitleBar(AppTitleBar);

        contentFrame.Navigate(_appearanceService.HomePageType);

        if (_settings.BackgroundMode == 3)
        {
            var sprite = await PipelineBuilder
                .FromBackdrop()
                .Blur(0, out EffectAnimation<float> blurAnimation)
                .AttachAsync(BackgroundImageBorder, BackgroundImageBorder);

            await blurAnimation(sprite.Brush, 0, TimeSpan.FromMilliseconds(1));
        }

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

    internal async void BlurAnimation(int from, int to)
    {
        var sprite = await PipelineBuilder
            .FromBackdrop()
            .Blur(from, out EffectAnimation<float> blurAnimation)
            .AttachAsync(BackgroundImageBorder, BackgroundImageBorder);

        await blurAnimation(sprite.Brush, to, TimeSpan.FromSeconds(0.1));
    }
}

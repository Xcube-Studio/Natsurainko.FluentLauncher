using CommunityToolkit.WinUI.Media.Pipelines;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Pages;
using Natsurainko.FluentLauncher.ViewModels;
using Natsurainko.FluentLauncher.Views.Home;
using System;
using System.Linq;
using Windows.Graphics;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ShellPage : Page, INavigationProvider
{
    public static XamlRoot _XamlRoot { get; private set; }
    public static Frame ContentFrame { get; private set; }

    object INavigationProvider.NavigationControl => contentFrame;
    private ShellViewModel VM => (ShellViewModel)DataContext;

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly AppearanceService _appearanceService = App.GetService<AppearanceService>();

    public ShellPage()
    {
        _appearanceService.ApplyBackgroundBeforePageInit(this);
        _appearanceService.ApplyThemeColorBeforePageInit(this);

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
        var pageTag = ((NavigationViewItem)args.InvokedItemContainer).Tag.ToString();

        if (pageTag == "HomePage" && _settings.UseNewHomePage)
            pageTag = "NewHomePage";

        VM.NavigationService.NavigateTo(pageTag);
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        => VM.NavigationService.GoBack();

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
        
        if (_settings.BackgroundMode == 3 && !VM._onNavigatedTo)
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
            if (App.GetService<IPageProvider>().RegisteredPages[item.Tag.ToString()].PageType == e.SourcePageType)
            {
                NavigationViewControl.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
            if (e.SourcePageType == typeof(NewHomePage) && item.Tag.ToString() == "HomePage")
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

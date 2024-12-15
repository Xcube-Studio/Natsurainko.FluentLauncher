using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels;
using Natsurainko.FluentLauncher.Views.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ShellPage : Page, INavigationProvider
{
    public static Frame ContentFrame { get; private set; } = null!; // Initialized on Page_Loaded

    object INavigationProvider.NavigationControl => contentFrame;

    private ShellViewModel VM => (ShellViewModel)DataContext;

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly SearchProviderService _searchProviderService = App.GetService<SearchProviderService>();

    private bool isUpdatingNavigationItemSelection = false;

    public ShellPage()
    {
        InitializeComponent();

        ConfigurePage();
    }

    #region Page Events

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.SetTitleBar(AppTitleBar);

        ConfigureNavigationView();
        UpdateTitleBarDragArea();
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.PreviousSize.Width <= 640 && e.NewSize.Width > 640)
        {
            NavViewPaneBackground.Translation += new System.Numerics.Vector3(48, 0, 0);
            TopNavViewPaneBackground.Translation -= new System.Numerics.Vector3(110, 0, 0);

            var PaneToggleButtonGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneToggleButtonGrid")!;
            PaneToggleButtonGrid.Translation -= new System.Numerics.Vector3(20, 0, 0);
        }

        if (e.PreviousSize.Width > 640 && e.NewSize.Width <= 640)
        {
            NavViewPaneBackground.Translation -= new System.Numerics.Vector3(48, 0, 0);
            TopNavViewPaneBackground.Translation += new System.Numerics.Vector3(110, 0, 0);

            var PaneToggleButtonGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneToggleButtonGrid")!;
            PaneToggleButtonGrid.Translation += new System.Numerics.Vector3(20, 0, 0);
        }

        Column2.MinWidth = e.NewSize.Width >= 680 ? 48 : 150;

        NavigationViewControl.PaneDisplayMode = e.NewSize.Width <= 640 ? NavigationViewPaneDisplayMode.LeftMinimal : NavigationViewPaneDisplayMode.LeftCompact;
        TopNavViewPaneToggleButtonsBorder.Width = e.NewSize.Width <= 640 ? 120 : 48;

        UpdateTitleBarDragArea();
    }

    #endregion

    #region NavigationView & Frame Events
    private void NavigationViewControl_PaneClosing(NavigationView sender, object _)
    {
        UpdateTitleBarDragArea();

        _settings.NavigationViewIsPaneOpen = false;

        NavViewPaneBackground.Opacity = 1;
        NavViewPaneBackground.Translation += new System.Numerics.Vector3(0, 0, 32);

        if (SearchBoxAreaGrid.Translation.Y < 0)
            SearchBoxAreaGrid.Translation += new System.Numerics.Vector3(0, 44, 0);
    }

    private void NavigationViewControl_PaneOpening(NavigationView sender, object _)
    {
        UpdateTitleBarDragArea();

        _settings.NavigationViewIsPaneOpen = true;

        NavViewPaneBackground.Opacity = 0;
        NavViewPaneBackground.Translation -= new System.Numerics.Vector3(0, 0, 32);

        if (this.ActualWidth < 1100)
            SearchBoxAreaGrid.Translation -= new System.Numerics.Vector3(0, 44, 0);
    }

    private void NavigationViewControl_BackRequested(NavigationView _, NavigationViewBackRequestedEventArgs args) => VM.NavigationService.GoBack();

    private void NavigationViewControl_ItemInvoked(NavigationView _, NavigationViewItemInvokedEventArgs args)
    {
        if (isUpdatingNavigationItemSelection)
            return;

        var pageTag = ((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()
            ?? throw new ArgumentNullException("The invoked item's tag is null.");

        VM.NavigationService.NavigateTo(pageTag);
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        isUpdatingNavigationItemSelection = true;

        foreach (NavigationViewItem item in NavigationViewControl.MenuItems.Union(NavigationViewControl.FooterMenuItems).Cast<NavigationViewItem>())
        {
            string tag = item.GetTag();

            if (App.GetService<IPageProvider>().RegisteredPages[tag].PageType == e.SourcePageType)
            {
                NavigationViewControl.SelectedItem = item;
                break;
            }
        }

        isUpdatingNavigationItemSelection = false;

        if (_settings.BackgroundMode == 3)
            BlurBorder.Opacity = typeof(HomePage).Equals(e.SourcePageType) ? 0 : 1;
        else BlurBorder.Opacity = 0;
    }

    #endregion

    #region TitleBar Controls Events

    private void AutoSuggestBox_Loaded(object sender, RoutedEventArgs e)
         => _searchProviderService.BindingSearchBox(AutoSuggestBox);

    #endregion

    #region Services Events
    private void BackgroundReloaded(object? sender, EventArgs e) => BlurBorder.Opacity = (_settings.BackgroundMode == 3) ? 1 : 0;

    private void UseBackgroundMaskChanged(SettingsContainer sender, SettingChangedEventArgs e)
    {
        NavViewPaneBackground.Visibility = _settings.UseBackgroundMask ? Visibility.Visible : Visibility.Collapsed;
        SearchBoxAreaBackgroundBorder.Visibility = _settings.UseBackgroundMask ? Visibility.Visible : Visibility.Collapsed;
        SearchBoxAreaGrid.Translation = _settings.UseBackgroundMask ? new System.Numerics.Vector3(0, 0, 16) : new System.Numerics.Vector3(0, 0, 0);
    }

    #endregion

    void ConfigurePage()
    {
        ContentFrame = contentFrame;
        NavigationViewControl.IsPaneOpen = _settings.NavigationViewIsPaneOpen;
        SearchBoxAreaBackgroundBorder.Visibility = _settings.UseBackgroundMask ? Visibility.Visible: Visibility.Collapsed;
        SearchBoxAreaGrid.Translation = _settings.UseBackgroundMask ? new System.Numerics.Vector3(0, 0, 16) : new System.Numerics.Vector3(0, 0, 0);
        BlurBorder.OpacityTransition = new ScalarTransition()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };

        App.GetService<AppearanceService>().BackgroundReloaded += BackgroundReloaded;
        _settings.UseBackgroundMaskChanged += UseBackgroundMaskChanged;
    }

    void ConfigureNavigationView()
    {
        NavViewPaneBackground.OpacityTransition = new ScalarTransition()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };
        NavViewPaneBackground.TranslationTransition = new Vector3Transition()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };
        TopNavViewPaneBackground.TranslationTransition = new Vector3Transition()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };
        SearchBoxAreaGrid.TranslationTransition = new Vector3Transition()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };

        NavViewPaneBackground.Visibility = _settings.UseBackgroundMask ? Visibility.Visible : Visibility.Collapsed;

        var RootSplitView = FindControl<SplitView>(NavigationViewControl, typeof(SplitView), "RootSplitView")!;
        RootSplitView.Margin = new Thickness(-1);
        RootSplitView.Padding = new Thickness(1);

        var PaneContentGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneContentGrid")!;
        PaneContentGrid.Padding = new Thickness(1,0,0,0);

        var PaneToggleButtonGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneToggleButtonGrid")!;
        PaneToggleButtonGrid.Translation += new System.Numerics.Vector3(20, 0, 0);
        PaneToggleButtonGrid.TranslationTransition = new Vector3Transition()
        {
            Duration = TimeSpan.FromMilliseconds(500)
        };
    }

    private void UpdateTitleBarDragArea()
    {
        var scaleAdjustment = XamlRoot.RasterizationScale;
        var height = (int)(48 * scaleAdjustment);

        if (AutoSuggestBox.Visibility == Visibility.Collapsed)
        {
            App.MainWindow.AppWindow.TitleBar.SetDragRectangles([ new()
            {
                X = (int)(Column0.ActualWidth * scaleAdjustment),
                Y = 0,
                Width = (int)((this.ActualWidth - Column0.ActualWidth) * scaleAdjustment),
                Height = height
            }]);

            return;
        }

        var transform = AutoSuggestBox.TransformToVisual(AppTitleBar);
        var absolutePosition = transform.TransformPoint(new Point(0, 0));

        var dragRects = new List<RectInt32>
        {
            new()
            {
                X = (int)(Column0.ActualWidth * scaleAdjustment),
                Y = 0,
                Width = (int)((absolutePosition.X - Column0.ActualWidth) * scaleAdjustment),
                Height = height
            },
            new()
            {
                X = (int)((absolutePosition.X + AutoSuggestBox.ActualWidth) * scaleAdjustment),
                Y = 0,
                Width = (int)((AppTitleBar.ActualWidth - (absolutePosition.X + AutoSuggestBox.ActualWidth)) * scaleAdjustment),
                Height = height
            }
        };

        App.MainWindow.AppWindow.TitleBar.SetDragRectangles([.. dragRects]);
    }

    private static T? FindControl<T>(UIElement parent, Type targetType, string ControlName) where T : FrameworkElement
    {
        if (parent == null) return null;

        if (parent.GetType() == targetType && ((T)parent).Name == ControlName)
        {
            return (T)parent;
        }
        T? result = null;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

            if (FindControl<T>(child, targetType, ControlName) != null)
            {
                result = FindControl<T>(child, targetType, ControlName);
                break;
            }
        }
        return result;
    }
}

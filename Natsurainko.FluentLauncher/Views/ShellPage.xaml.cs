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
using System.ComponentModel;
using System.Linq;
using Windows.Graphics;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ShellPage : Page, INavigationProvider, INotifyPropertyChanged
{
    ShellViewModel VM => (ShellViewModel)DataContext;

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly SearchProviderService _searchProviderService = App.GetService<SearchProviderService>();
    private readonly IPageProvider pageProvider = App.GetService<IPageProvider>();

    private bool isUpdatingNavigationItemSelection = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    object INavigationProvider.NavigationControl => contentFrame;
    INavigationService INavigationProvider.NavigationService => VM.NavigationService;

    public bool CanGoBack
    {
        get
        {
            if (contentFrame.Content is INavigationProvider childPage)
                return contentFrame.CanGoBack || childPage.NavigationService.CanGoBack;
            else
                return contentFrame.CanGoBack;
        }
    }

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

            //var PaneToggleButtonGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneToggleButtonGrid")!;
            //PaneToggleButtonGrid.Translation -= new System.Numerics.Vector3(20, 0, 0);
        }

        if (e.PreviousSize.Width > 640 && e.NewSize.Width <= 640)
        {
            NavViewPaneBackground.Translation -= new System.Numerics.Vector3(48, 0, 0);
            TopNavViewPaneBackground.Translation += new System.Numerics.Vector3(110, 0, 0);

            //var PaneToggleButtonGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneToggleButtonGrid")!;
            //PaneToggleButtonGrid.Translation += new System.Numerics.Vector3(20, 0, 0);
        }

        NavigationViewControl.PaneDisplayMode = e.NewSize.Width <= 640 ? NavigationViewPaneDisplayMode.LeftMinimal : NavigationViewPaneDisplayMode.LeftCompact;
        TopNavViewPaneToggleButtonsBorder.Width = e.NewSize.Width <= 640 ? 84 : 48;

        UpdateSearchBoxArea();

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

    private void NavigationViewControl_BackRequested(NavigationView _, NavigationViewBackRequestedEventArgs args)
    {
        if (contentFrame.Content is INavigationProvider childPage && childPage.NavigationService.CanGoBack)
        {
            childPage.NavigationService.GoBack();
        }
        else
        {
            VM.NavigationService.GoBack();
        }
        OnPropertyChanged(nameof(CanGoBack));
    }

    private void NavigationViewControl_ItemInvoked(NavigationView _, NavigationViewItemInvokedEventArgs args)
    {
        if (isUpdatingNavigationItemSelection)
            return;

        NavigationViewItem navigationViewItem = ((NavigationViewItem)args.InvokedItemContainer);

        if (!navigationViewItem.SelectsOnInvoked) return;

        string pageTag = navigationViewItem.Tag.ToString() 
            ?? throw new ArgumentNullException("The invoked item's tag is null.");

        VM.NavigationService.NavigateTo(pageTag);
        OnPropertyChanged(nameof(CanGoBack));
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        isUpdatingNavigationItemSelection = true;

        void EnumerateTreeMenuItems(IEnumerable<object> menuItems)
        {
            foreach (object item in menuItems)
            {
                if (item is not NavigationViewItem navigationViewItem) 
                    continue;

                string? tag = navigationViewItem.GetTag();

                if (tag is not null && pageProvider.RegisteredPages[tag].PageType == e.SourcePageType)
                {
                    NavigationViewControl.SelectedItem = navigationViewItem;
                    break;
                }

                if (navigationViewItem.MenuItems != null && navigationViewItem.MenuItems.Count > 0)
                    EnumerateTreeMenuItems(navigationViewItem.MenuItems);
            }
        }

        EnumerateTreeMenuItems(NavigationViewControl.MenuItems.Union(NavigationViewControl.FooterMenuItems));

        isUpdatingNavigationItemSelection = false;

        if (_settings.BackgroundMode == 3)
            BlurBorder.Opacity = typeof(HomePage).Equals(e.SourcePageType) ? 0 : 1;
        else BlurBorder.Opacity = 0;

        UpdateSearchBoxArea();
        OnPropertyChanged(nameof(CanGoBack));
    }

    #endregion

    #region TitleBar Controls Events

    private void AutoSuggestBox_Loaded(object sender, RoutedEventArgs e)
    {
        _searchProviderService.BindingSearchBox(AutoSuggestBox);
        UpdateSearchBoxArea();
    }

    #endregion

    #region Services Events
    private void BackgroundReloaded(object? sender, EventArgs e) => BlurBorder.Opacity = (_settings.BackgroundMode == 3) ? 1 : 0;

    private void UseBackgroundMaskChanged(SettingsContainer sender, SettingChangedEventArgs e)
    {
        NavViewPaneBackground.Visibility = 
        TopNavViewPaneBackground.Visibility = 
        SearchBoxAreaBackgroundBorder.Visibility = 
            _settings.UseBackgroundMask ? Visibility.Visible : Visibility.Collapsed;

        SearchBoxAreaGrid.Shadow = _settings.UseBackgroundMask ? SharedShadow : null;
    }

    #endregion

    void ConfigurePage()
    {
        NavigationViewControl.IsPaneOpen = _settings.NavigationViewIsPaneOpen;
        SearchBoxAreaGrid.Shadow = _settings.UseBackgroundMask ? SharedShadow : null;

        NavViewPaneBackground.Visibility =
        TopNavViewPaneBackground.Visibility =
        SearchBoxAreaBackgroundBorder.Visibility =
            _settings.UseBackgroundMask ? Visibility.Visible : Visibility.Collapsed;

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
        SearchBoxAreaBackgroundBorder.OpacityTransition = new ScalarTransition()
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

        var RootSplitView = FindControl<SplitView>(NavigationViewControl, typeof(SplitView), "RootSplitView")!;
        RootSplitView.Margin = new Thickness(-1);
        RootSplitView.Padding = new Thickness(1);

        var PaneContentGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneContentGrid")!;
        PaneContentGrid.Padding = new Thickness(1,0,0,0);

        //var PaneToggleButtonGrid = FindControl<Grid>(NavigationViewControl, typeof(Grid), "PaneToggleButtonGrid")!;
        //PaneToggleButtonGrid.Translation += new System.Numerics.Vector3(20, 0, 0);
        //PaneToggleButtonGrid.TranslationTransition = new Vector3Transition()
        //{
        //    Duration = TimeSpan.FromMilliseconds(500)
        //};

#if ENABLE_LOAD_EXTENSIONS
        foreach (var extension in App.GetService<List<global::FluentLauncher.Infra.ExtensionHost.Extensions.IExtension>>())
        {
            if (extension is global::FluentLauncher.Infra.ExtensionHost.Extensions.INavigationProviderExtension provider)
            {
                foreach (var item in provider.ProvideNavigationItems())
                {
                    NavigationViewControl.MenuItems.Add(item);
                }
            }
        }
#endif
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

        if (NavigationViewControl.IsPaneOpen)
        {
            // Allow dragging over the entire title bar when the search box is hidden
            var rect = new RectInt32()
            {
                X = (int)(Column0.ActualWidth * scaleAdjustment),
                Y = 0,
                Width = (int)((AppTitleBar.ActualWidth - Column0.ActualWidth) * scaleAdjustment),
                Height = height
            };
            App.MainWindow.AppWindow.TitleBar.SetDragRectangles([rect]);
        }
        else
        {
            var rectLeft = new RectInt32()
            {
                X = (int)(Column0.ActualWidth * scaleAdjustment),
                Y = 0,
                Width = (int)((SearchBoxAreaGrid.Margin.Left - Column0.ActualWidth + AutoSuggestBox.Margin.Left) * scaleAdjustment),
                Height = height
            };
            var rectRight = new RectInt32()
            {
                X = (int)((AppTitleBar.ActualWidth - SearchBoxAreaGrid.Margin.Right - AutoSuggestBox.Margin.Right) * scaleAdjustment),
                Y = 0,
                Width = (int)((SearchBoxAreaGrid.Margin.Right + AutoSuggestBox.Margin.Right) * scaleAdjustment),
                Height = height
            };
            App.MainWindow.AppWindow.TitleBar.SetDragRectangles([rectLeft, rectRight]);
        }
    }

    private void UpdateSearchBoxArea()
    {
        if (ActualWidth <= 640 || typeof(HomePage).Equals(contentFrame.Content.GetType()))
        {
            SearchBoxAreaGrid.Translation = new System.Numerics.Vector3(0, 0, 16);
            SearchBoxAreaBackgroundBorder.Opacity = 1;
        }
        else
        {
            SearchBoxAreaGrid.Translation = new System.Numerics.Vector3(0, 0, 0);
            SearchBoxAreaBackgroundBorder.Opacity = 0;
        }

        // Update search box margin to ensure space from the buttons on both sides
        double leftBound = Column0.ActualWidth + 32;
        double rightBound = Column2.ActualWidth + 32;
        double totalWidth = AppTitleBar.ActualWidth;
        const double searchBoxMaxWidth = 380;

        // Center the search box while keeping enough space
        double center = totalWidth / 2;
        double leftMargin = Math.Max(center - searchBoxMaxWidth / 2, leftBound);
        double rightMargin = Math.Max(totalWidth - (center + searchBoxMaxWidth / 2), rightBound);

        // Set width using left and right margin
        SearchBoxAreaGrid.Margin = new Thickness(leftMargin, 0, rightMargin, 0);
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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        if (args.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            appTitleGrid.Margin = new Thickness(16, 0, 0, 0);
        }
        else
        {
            appTitleGrid.Margin = new Thickness(8, -80, 0, 0);
        }
    }
}

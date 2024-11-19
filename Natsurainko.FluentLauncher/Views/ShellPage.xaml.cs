using CommunityToolkit.WinUI.Media;
using CommunityToolkit.WinUI.Media.Pipelines;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ShellPage : Page, INavigationProvider
{
    public static Frame ContentFrame { get; private set; } = null!; // Initialized on Page_Loaded

    object INavigationProvider.NavigationControl => contentFrame;

    private ShellViewModel VM => (ShellViewModel)DataContext;

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly SearchProviderService _searchProviderService = App.GetService<SearchProviderService>();

    private bool isUpdatingNavigationItemSelection = false;
    private int backgroundBlurredValue = 0;

    public ShellPage()
    {
        InitializeComponent();

        ConfigurePage();
    }

    #region Page Events

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.SetTitleBar(AppTitleBar);
        ConfigureNavigationView();

        if (_settings.UseBackgroundMask)
        {
            AppTitleBar.Background = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
            AppTitleBar.RequestedTheme = ElementTheme.Light;
        }

        if (_settings.BackgroundMode == 3 && !VM._onNavigatedTo)
        {
            var sprite = await PipelineBuilder
                .FromBackdrop()
                .Blur(0, out EffectAnimation<float> blurAnimation)
                .AttachAsync(BlurBorder, BlurBorder);

            await blurAnimation(sprite.Brush, 0, TimeSpan.FromMilliseconds(1));
        }

        UpdateTitleBarDragArea();
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        AppTitle.Visibility = e.NewSize.Width <= 750 ? Visibility.Collapsed : Visibility.Visible;

        UpdateTitleBarDragArea();
    }

    private void Page_ActualThemeChanged(FrameworkElement sender, object args)
    {
        if (_settings.BackgroundMode == 3 || _settings.BackgroundMode == 2)
        {
            BackgroundContentBorder.Background = null;
            BackgroundContentBorder.BorderBrush = null;
        } 
        else
        {
            BackgroundContentBorder.Background = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                : new SolidColorBrush(Color.FromArgb(76, 58, 58, 58));
            BackgroundContentBorder.BorderBrush = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(15, 0, 0, 0))
                : new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));
        }
    }

    #endregion

    #region NavigationView & Frame Events
    private void NavigationViewControl_PaneClosing(NavigationView sender, object _)
    {
        AutoSuggestBox.Visibility = Visibility.Visible;

        UpdateTitleTextPosition(sender);
        UpdateTitleBarDragArea();

        _settings.NavigationViewIsPaneOpen = false;
    }

    private void NavigationViewControl_PaneOpening(NavigationView sender, object _)
    {
        AutoSuggestBox.Visibility = NavigationViewControl.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Collapsed : Visibility.Visible;

        UpdateTitleTextPosition(sender);
        UpdateTitleBarDragArea();

        _settings.NavigationViewIsPaneOpen = true;
    }

    private void NavigationViewControl_ItemInvoked(NavigationView _, NavigationViewItemInvokedEventArgs args)
    {
        if (isUpdatingNavigationItemSelection)
            return;

        var pageTag = ((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()
            ?? throw new ArgumentNullException("The invoked item's tag is null.");

        VM.NavigationService.NavigateTo(pageTag);
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        PaneToggleButton.Visibility = args.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Visible : Visibility.Collapsed;
        NavigationViewControl.IsPaneToggleButtonVisible = args.DisplayMode != NavigationViewDisplayMode.Minimal;

        if (args.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            Grid.SetRow(NavigationViewControl, 0);
            Grid.SetRowSpan(NavigationViewControl, 2);
            Spacer.Height = 48;
            contentFrame.Margin = new Thickness(0, 48, 0, 0);
        }
        else
        {
            Grid.SetRow(NavigationViewControl, 1);
            Grid.SetRowSpan(NavigationViewControl, 1);
            Spacer.Height = 0;
            contentFrame.Margin = new Thickness(0);
        }

        UpdateTitleTextPosition(sender);
        UpdateTitleBarDragArea();
    }

    private async void ContentFrame_Navigated(object sender, NavigationEventArgs e)
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
            await BlurAnimation(!typeof(HomePage).Equals(e.SourcePageType) ? 75 : 0);
        else await BlurAnimation(0);
    }

    #endregion

    #region TitleBar Controls Events

    private void PaneToggleButton_Click(object sender, RoutedEventArgs e)
        => NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;

    private void BackButton_Click(object sender, RoutedEventArgs e)
        => VM.NavigationService.GoBack();

    private void AutoSuggestBox_Loaded(object sender, RoutedEventArgs e)
         => _searchProviderService.BindingSearchBox(AutoSuggestBox);

    #endregion

    #region AppearanceService Events
    private async void BackgroundReloaded(object? sender, EventArgs e)
    {
        await BlurAnimation((_settings.BackgroundMode == 3) ? 75 : 0, 0.001);

        if (_settings.BackgroundMode == 3 || _settings.BackgroundMode == 2)
        {
            BackgroundContentBorder.Background = null;
            BackgroundContentBorder.BorderBrush = null;
        }
        else
        {
            BackgroundContentBorder.Background = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                : new SolidColorBrush(Color.FromArgb(76, 58, 58, 58));
            BackgroundContentBorder.BorderBrush = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(15, 0, 0, 0))
                : new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));
        }
    }

    #endregion

    void ConfigurePage()
    {
        ContentFrame = contentFrame;
        NavigationViewControl.IsPaneOpen = _settings.NavigationViewIsPaneOpen;

        App.GetService<AppearanceService>().BackgroundReloaded += BackgroundReloaded;
    }

    void ConfigureNavigationView()
    {
        if (_settings.UseBackgroundMask)
        {
            var RootSplitView = FindControl<SplitView>(NavigationViewControl, typeof(SplitView), "RootSplitView");
            if (RootSplitView != null)
            {
                RootSplitView.CornerRadius = new CornerRadius(0);

                var PaneContentGrid = FindControl<Grid>(RootSplitView, typeof(Grid), "PaneContentGrid")!;

                var Border = new Border();
                Border.Background = new BackdropBlurBrush() { Amount = 16 };
                Grid.SetRowSpan(Border, 8);

                PaneContentGrid.Children.Insert(0, Border);
                PaneContentGrid.Translation += new System.Numerics.Vector3(0, 0, 48);

                PaneContentGrid.Background = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                    : new SolidColorBrush(Color.FromArgb(76, 58, 58, 58));
                PaneContentGrid.BorderBrush = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(15, 0, 0, 0))
                    : new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));

                PaneContentGrid.ActualThemeChanged += (_, e) =>
                {
                    PaneContentGrid.Background = this.ActualTheme == ElementTheme.Light
                        ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                        : new SolidColorBrush(Color.FromArgb(76, 58, 58, 58));
                    PaneContentGrid.BorderBrush = this.ActualTheme == ElementTheme.Light
                        ? new SolidColorBrush(Color.FromArgb(15, 0, 0, 0))
                        : new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));
                };
            }
        }
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

    private void UpdateTitleTextPosition(NavigationView sender)
    {
        AppTitle.TranslationTransition = new Vector3Transition();
        AppTitle.Translation = ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                 sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                 ? new System.Numerics.Vector3(8, 0, 0)
                 : new System.Numerics.Vector3(28, 0, 0);
    }

    private async Task BlurAnimation(int to, double time = 0.1)
    {
        //if (backgroundBlurredValue.Equals(to))
        //    return;

        var sprite = await PipelineBuilder
            .FromBackdrop()
            .Blur(backgroundBlurredValue, out EffectAnimation<float> blurAnimation)
            .AttachAsync(BlurBorder, BlurBorder);

        await blurAnimation(sprite.Brush, to, TimeSpan.FromSeconds(time));
        backgroundBlurredValue = to;
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

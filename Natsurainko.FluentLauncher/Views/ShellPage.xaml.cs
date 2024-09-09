using CommunityToolkit.WinUI.Media.Pipelines;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels;
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
    private readonly AppearanceService _appearanceService = App.GetService<AppearanceService>();
    private readonly SearchProviderService _searchProviderService = App.GetService<SearchProviderService>();

    private bool isUpdatingNavigationItemSelection = false;

    public ShellPage()
    {
        _appearanceService.ApplyBackgroundBeforePageInit(this);
        _appearanceService.ApplySettingsBeforePageInit();

        InitializeComponent();

        ContentFrame = contentFrame;
        _appearanceService.ApplyBackgroundAfterPageInit(this);
        _appearanceService.RegisterNavigationView(NavigationViewControl);
    }

    #region Page Events

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.SetTitleBar(AppTitleBar);

        if (_settings.BackgroundMode == 3 && !VM._onNavigatedTo)
        {
            var sprite = await PipelineBuilder
                .FromBackdrop()
                .Blur(0, out EffectAnimation<float> blurAnimation)
                .AttachAsync(BackgroundImageBorder, BackgroundImageBorder);

            await blurAnimation(sprite.Brush, 0, TimeSpan.FromMilliseconds(1));
        }

        UpdateTitleBarDragArea();
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        AppTitle.Visibility = e.NewSize.Width <= 750 ? Visibility.Collapsed : Visibility.Visible;

        UpdateTitleBarDragArea();
    }

    #endregion

    #region NavigationView & Frame Events
    private void NavigationViewControl_PaneClosing(NavigationView sender, object _)
    {
        AutoSuggestBox.Visibility = Visibility.Visible;

        UpdateTitleTextPosition(sender);
        UpdateTitleBarDragArea();
    }

    private void NavigationViewControl_PaneOpening(NavigationView sender, object _)
    {
        AutoSuggestBox.Visibility = NavigationViewControl.DisplayMode == NavigationViewDisplayMode.Minimal ? Visibility.Collapsed : Visibility.Visible;

        UpdateTitleTextPosition(sender);
        UpdateTitleBarDragArea();
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

    internal async void BlurAnimation(int from, int to)
    {
        var sprite = await PipelineBuilder
            .FromBackdrop()
            .Blur(from, out EffectAnimation<float> blurAnimation)
            .AttachAsync(BackgroundImageBorder, BackgroundImageBorder);

        await blurAnimation(sprite.Brush, to, TimeSpan.FromSeconds(0.1));
    }
}

using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.View.Pages;
using Natsurainko.FluentLauncher.View.Pages.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher;

public sealed partial class MainContainer : Page
{
    private static ListBox InformationListBox { get; set; }

    public static Frame ContentFrame { get; private set; }

    public MainContainer()
    {
        this.InitializeComponent();

        ContentFrame = contentFrame;
        InformationListBox = InformationList;
        SharedShadow.Receivers.Add(BackgroundGrid);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        GlobalResources.SetResourceLoader(ResourceLoader.GetForCurrentView());

        var coreApplicationViewTitleBar = CoreApplication.GetCurrentView().TitleBar;
        coreApplicationViewTitleBar.LayoutMetricsChanged += (s, args) => UpdateAppTitle(s);
        coreApplicationViewTitleBar.ExtendViewIntoTitleBar = true;

        Window.Current.SetTitleBar(AppTitleBar);

        var applicationViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
        applicationViewTitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;
        applicationViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
        applicationViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        if (Environment.OSVersion.Version.Build >= 22000)
            BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
        else this.Background = (AcrylicBrush)this.Resources["SystemControlAcrylicWindowBrush"];

        contentFrame.Navigate(typeof(HomePage));
    }

    private void UpdateAppTitleMargin(Muxc.NavigationView sender)
    {
        const int smallLeftIndent = 4, largeLeftIndent = 24;

        AppTitle.TranslationTransition = new Vector3Transition();

        if ((sender.DisplayMode == Muxc.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                 sender.DisplayMode == Muxc.NavigationViewDisplayMode.Minimal)
        {
            AppTitle.Translation = new System.Numerics.Vector3(smallLeftIndent, 0, 0);
        }
        else
        {
            AppTitle.Translation = new System.Numerics.Vector3(largeLeftIndent, 0, 0);
        }
    }

    private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
    {
        Thickness currMargin = AppTitleBar.Margin;
        AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
    }

    private void NavigationViewControl_PaneClosing(Muxc.NavigationView sender, object _)
    => UpdateAppTitleMargin(sender);

    private void NavigationViewControl_PaneOpening(Muxc.NavigationView sender, object _)
        => UpdateAppTitleMargin(sender);

    private void NavigationViewControl_ItemInvoked(Muxc.NavigationView _, Muxc.NavigationViewItemInvokedEventArgs args)
        => contentFrame.Navigate(args.IsSettingsInvoked ? typeof(SettingsPage) : (Type.GetType($"Natsurainko.FluentLauncher.View.Pages.{((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag ??= string.Empty}")) ?? typeof(HomePage));

    private void NavigationViewControl_BackRequested(Muxc.NavigationView sender, Muxc.NavigationViewBackRequestedEventArgs args)
        => contentFrame.GoBack();

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (Muxc.NavigationViewItem item in NavigationViewControl.MenuItems.Union(NavigationViewControl.FooterMenuItems).Cast<Muxc.NavigationViewItem>())
        {
            if ((string)item.Tag == e.SourcePageType.Name)
            {
                NavigationViewControl.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
        }
    }

    private void NavigationViewControl_DisplayModeChanged(Muxc.NavigationView sender, Muxc.NavigationViewDisplayModeChangedEventArgs args)
    {
        GlobalResources.SetNavigationViewDisplayMode(sender.DisplayMode);

        var currMargin = AppTitleBar.Margin;
        AppTitleBar.Margin = new Thickness(sender.DisplayMode == Muxc.NavigationViewDisplayMode.Minimal ? (sender.CompactPaneLength * 2) : sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
        UpdateAppTitleMargin(sender);

        contentFrame.Margin = new Thickness(0, sender.DisplayMode == Muxc.NavigationViewDisplayMode.Minimal ? -50 : -80, 0, 0);
    }

    public static void ShowInfoBarAsync(string title, string message = "", InfoBarSeverity severity = InfoBarSeverity.Informational, int delay = 5000, ButtonBase button = null) => DispatcherHelper.RunAsync(async () =>
    {
        var viewData = new InformationViewData
        {
            Button = button,
            Delay = delay,
            Description = message,
            Title = title,
            Severity = severity
        };

        InformationListBox.Items.Add(viewData);
        await Task.Delay(delay);

        if (!viewData.Removed)
            InformationListBox.Items.Remove(viewData);
    });

    private void InfoBar_CloseButtonClick(InfoBar sender, object args)
    {
        var viewData = sender.DataContext as InformationViewData;
        viewData.Removed = true;

        InformationList.Items.Remove(viewData);
    }

    private void InfoBar_Loaded(object sender, RoutedEventArgs e)
        => ((InfoBar)sender).Translation += new System.Numerics.Vector3(0, 0, 32);
}

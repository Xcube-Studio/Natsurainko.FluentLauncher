
/* 项目“Natsurainko.FluentLauncher (SelfContained)”的未合并的更改
在此之前:
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
在此之后:
using Microsoft.UI;
*/
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

/* 项目“Natsurainko.FluentLauncher (SelfContained)”的未合并的更改
在此之前:
using Windows.ApplicationModel.Core;
using Windows.Graphics;
using WinUIEx;
using System.Threading.Tasks;
using Natsurainko.FluentLauncher.Models;
using Microsoft.UI;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentCore.Module.Installer;
在此之后:
using Natsurainko.FluentCore.Module.Installer;
using Natsurainko.FluentCore.Module.Mod;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.FluentLauncher.Models;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
*/
using Natsurainko.FluentCore.Module.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/* 项目“Natsurainko.FluentLauncher (SelfContained)”的未合并的更改
在此之前:
using Windows.UI;
using Natsurainko.FluentCore.Module.Mod;
在此之后:
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI;
using WinUIEx;
*/
using Windows.Graphics;

namespace Natsurainko.FluentLauncher.Views.Pages;

public sealed partial class MainContainer : Page
{
    private static ListBox InformationListBox { get; set; }

    public static XamlRoot _XamlRoot { get; private set; }

    public static Frame ContentFrame { get; private set; }

    public MainContainer()
    {
        /*
        this.Resources.Add("NavigationViewContentBackground", new SolidColorBrush(Colors.Transparent));
        this.Resources.Add("NavigationViewPaneContentGridMargin", new Thickness(-1, 0, -1, 0));
        this.Resources.Add("NavigationViewContentGridCornerRadius", new CornerRadius(0));
        */
        InitializeComponent();

        InformationListBox = InformationList;
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
        contentFrame.Navigate(typeof(Home));
        CurseForgeApi.InitApiKey("$2a$10$Awb53b9gSOIJJkdV3Zrgp.CyFP.dI13QKbWn/4UZI4G4ff18WneB6");

        RefreshDragArea();
    }

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
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RefreshDragArea();

        App.Configuration.AppWindowWidth = App.MainWindow.Width;
        App.Configuration.AppWindowHeight = App.MainWindow.Height;
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

    private void InfoBar_CloseButtonClick(InfoBar sender, object args)
    {
        var messageData = sender.DataContext as MessageData;
        messageData.Removed = true;

        InformationList.Items.Remove(messageData);
    }

    private void InfoBar_Loaded(object sender, RoutedEventArgs e)
        => ((InfoBar)sender).Translation += new System.Numerics.Vector3(0, 0, 32);

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

    public static void ShowMessagesAsync(
        string title,
        string message = "",
        InfoBarSeverity severity = InfoBarSeverity.Informational,
        int delay = 5000,
        ButtonBase button = null)
    => App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
    {
        if (InformationListBox == null)
            return;

        var obj = new MessageData
        {
            Button = button,
            Message = message,
            Title = title,
            Severity = severity,
            Removed = false
        };

        InformationListBox.Items.Add(obj);
        await Task.Delay(delay);

        if (!obj.Removed)
            InformationListBox.Items.Remove(obj);
    });

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
}

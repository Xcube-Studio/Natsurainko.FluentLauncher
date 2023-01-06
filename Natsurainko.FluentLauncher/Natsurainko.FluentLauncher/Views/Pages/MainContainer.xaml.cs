using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;
using Windows.Graphics;
using WinUIEx;
using System.Threading.Tasks;
using Natsurainko.FluentLauncher.Models;
using Microsoft.UI;

namespace Natsurainko.FluentLauncher.Views.Pages;

public sealed partial class MainContainer : Page
{
    private static ListBox InformationListBox { get; set; }

    public static XamlRoot _XamlRoot { get; private set; }

    public static Frame ContentFrame { get; private set; }

    public MainContainer()
    {
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

        App.MainWindow.Backdrop = Environment.OSVersion.Version.Build >= 22000
           ? new MicaSystemBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
           : new AcrylicSystemBackdrop() 
           { 
               DarkTintOpacity = 0.75,
               DarkLuminosityOpacity = 0.75,
               DarkTintColor = Colors.Black,
               DarkFallbackColor = Colors.Black  
           };
        
        App.MainWindow.SetTitleBar(AppTitleBar);
        contentFrame.Navigate(typeof(Home));

        RefreshDragArea();
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
}

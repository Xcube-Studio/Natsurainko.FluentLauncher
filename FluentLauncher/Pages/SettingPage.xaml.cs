using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e) => Initialize();
        #endregion

        #region NavigationView
        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args) => this.Frame.Navigate(typeof(MainContainer));

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case "About":
                case "关于":
                    _ = contentFrame.Navigate(typeof(AboutPage));
                    break;
                case "Basic Settings":
                case "基础设置":
                    _ = contentFrame.Navigate(typeof(BasicSettingsPage));
                    break;
                case "Account":
                case "账户":
                    _ = contentFrame.Navigate(typeof(AccountPage));
                    break;
                case "Download":
                case "下载":
                    _ = contentFrame.Navigate(typeof(DownloadPage));
                    break;
                default:
                    break;
            }
        }
        #endregion

        public void Initialize()
        {
            ShareResource.InfoBar = InfoBar;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;

            if (!ShareResource.WebBrowserNavigateBack)
            {
                MainNavigationViewItem.IsSelected = true;
                contentFrame.Navigate(typeof(BasicSettingsPage));
            }
            else ShareResource.WebBrowserNavigateBack = false;
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        #endregion
    }
}

using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
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
    public sealed partial class WebBrowser : Page
    {
        public WebBrowser()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;
        }
        #endregion

        #region WebView2
        private void WebView_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            if (WebView.Source.AbsoluteUri == "about:blank")
                return;

            if (WebView.Source.AbsoluteUri.Split("?")[1].StartsWith("error"))
            {
                ShareResource.WebBrowserNavigateBack = true;
                this.Frame.Navigate(typeof(SettingPage));
                _ = ShareResource.ShowInfoAsync("Login Failed", WebView.Source.AbsoluteUri.Split("?")[1].Replace("%20", " "), 5000, InfoBarSeverity.Error);
            }
            if (WebView.Source.AbsoluteUri.Split("?")[1].StartsWith("code"))
            {
                ShareResource.WebBrowserNavigateBack = true;
                ShareResource.WebBrowserLoginCode = WebView.Source.AbsoluteUri.Split("=")[1];
                this.Frame.Navigate(typeof(SettingPage));
            }

            GC.Collect();
        }
        #endregion

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        #endregion
    }
}

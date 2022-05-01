using FluentLauncher.Models;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;
using System;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using FluentLauncher.Converters;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Media.Playback;

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
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        #endregion

        #region NavigationView
        private void NavigationView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args) => ShareResource.AppTitleVisible = true;

        private void NavigationView_PaneClosing(Microsoft.UI.Xaml.Controls.NavigationView sender, object args) => ShareResource.AppTitleVisible = false;

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args) => this.Frame.Navigate(typeof(MainPage));

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
                case "Theme":
                case "主题":
                    _ = contentFrame.Navigate(typeof(ThemePage));
                    break;
                default:
                    break;
            }

            UpdateInfoBadge();
        }
        #endregion

        public void Initialize()
        {
            if (!ShareResource.WebBrowserNavigateBack)
            {
                MainNavigationViewItem.IsSelected = true;
                contentFrame.Navigate(typeof(BasicSettingsPage));
            }
            else ShareResource.WebBrowserNavigateBack = false;

            UpdateInfoBadge();
        }

        private void UpdateInfoBadge()
        {
            if (ShareResource.GetBasicSettingsProblem())
                BasicInfoBadge.Visibility = Visibility.Visible;
            else BasicInfoBadge.Visibility = Visibility.Collapsed;

            if (ShareResource.GetAccountProblem())
                AccountInfoBadge.Visibility = Visibility.Visible;
            else AccountInfoBadge.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}

using FluentCore.Service.Network;
using FluentLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        #region NavigationView
        private void NavigationView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args) => ShareResource.AppTitleVisible = true;

        private void NavigationView_PaneClosing(Microsoft.UI.Xaml.Controls.NavigationView sender, object args) => ShareResource.AppTitleVisible = false;

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case "Home":
                case "主页":
                    _ = contentFrame.Navigate(typeof(HomePage));
                    break;
                case "News":
                case "新闻":
                    _ = contentFrame.Navigate(typeof(NewsPage));
                    break;
                case "Cores":
                case "游戏核心":
                    _ = contentFrame.Navigate(typeof(CoresPage));
                    break;
                case "Console":
                case "控制台":
                    _ = contentFrame.Navigate(typeof(ConsolePage));
                    break;
                case "Settings":
                case "设置":
                    _ = this.Frame.Navigate(typeof(SettingPage));
                    break;
                default:
                    break;
            }

            UpdateInfoBadge();
        }

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args) => contentFrame.GoBack();
        #endregion

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(HomePage));
            HomeNavigationViewItem.IsSelected = true;

            UpdateInfoBadge();
        }
        #endregion

        private void UpdateInfoBadge()
        {
            if (ShareResource.GetBasicSettingsProblem() || ShareResource.GetAccountProblem())
                InfoBadge.Visibility = Visibility.Visible;
            else InfoBadge.Visibility = Visibility.Collapsed;
        }
    }
}

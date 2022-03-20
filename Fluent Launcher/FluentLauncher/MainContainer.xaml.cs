using FluentCore.Service.Network;
using FluentLauncher.Models;
using FluentLauncher.Pages;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainContainer : Page
    {
        public MainContainer()
        {
            ShareResource.DownloadNews = BeginDownloadNewsAsync();
            ShareResource.InitializeNewsPicture = ShareResource.BeginInitializeNewsPicture();
            this.InitializeComponent();
        }

        #region UI

        #region NavigationView
        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case "Home":
                case "主页":
                    _ = contentFrame.Navigate(typeof(MainPage));
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
            ShareResource.InfoBar = InfoBar;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, args) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;

            contentFrame.Navigate(typeof(MainPage));
            HomeNavigationViewItem.IsSelected = true;

            UpdateInfoBadge();
        }
        #endregion

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private async Task BeginDownloadNewsAsync()
        {
            var res = await HttpHelper.HttpGetAsync("https://launchercontent.mojang.com/news.json");
            ShareResource.News = JsonConvert.DeserializeObject<MojangNews>(await res.Content.ReadAsStringAsync());

            for (int i = 0; i < ShareResource.News.Entries.Count; i++)
            {
                var entry = ShareResource.News.Entries[i];
                if (entry.PlayPageImage != null)
                    ShareResource.News.Entries[i].PlayPageImage.Url = $"https://launchercontent.mojang.com{ShareResource.News.Entries[i].PlayPageImage.Url}";

                ShareResource.News.Entries[i].Date = Convert.ToDateTime(entry.Date, new DateTimeFormatInfo() { ShortDatePattern = "yyyy-MM-dd" })
                    .ToString("MMMM dd, yyyy", CultureInfo.CreateSpecificCulture("en-GB"));
            }
        }

        private void UpdateInfoBadge()
        {
            if (ShareResource.GetBasicSettingsProblem() || ShareResource.GetAccountProblem())
                InfoBadge.Visibility = Visibility.Visible;
            else InfoBadge.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}

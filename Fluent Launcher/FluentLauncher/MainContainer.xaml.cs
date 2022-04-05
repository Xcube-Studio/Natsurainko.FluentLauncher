using FluentCore.Service.Network;
using FluentLauncher.Models;
using FluentLauncher.Pages;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;
using FluentLauncher.Converters;

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

            ShareResource.MainContainer = this;
            /*
            var player = new Windows.Media.Playback.MediaPlayer()
            {
                Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/test.mp4"))
            };
            player.CommandManager.IsEnabled = false;
            player.IsLoopingEnabled = true;
            player.Play();

            MediaPlayerElement.SetMediaPlayer(player);*/
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
            _ = this.Dispatcher.RunAsync(default, delegate
            {
                LoadBackground();
            });

            ShareResource.InfoBar = InfoBar;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, args) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;

            contentFrame.Navigate(typeof(MainPage));
            HomeNavigationViewItem.IsSelected = true;

            UpdateInfoBadge();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MediaPlayerElement.SetMediaPlayer(null);
            MediaPlayerElement.Source = null;
        }
        #endregion

        private void LoadBackground()
        {
            RequestedTheme = ShareResource.SelectedTheme.ElementTheme;

            Background = new SolidColorBrush(Colors.Transparent);
            BackgroundImage.Source = null;
            MediaPlayerElement.SetMediaPlayer(null);
            MediaPlayerElement.Source = null;

            switch (ShareResource.SelectedTheme.BackgroundType)
            {
                case BackgroundType.Normal:
                    switch (ShareResource.SelectedTheme.ElementTheme)
                    {
                        case ElementTheme.Default:
                        case ElementTheme.Light:
                        default:
                            Background = new SolidColorBrush(Colors.White);
                            break;
                        case ElementTheme.Dark:
                            Background = new SolidColorBrush(ColorConverter.FromString("#202020"));
                            break;
                    }
                    break;
                case BackgroundType.Acrylic:
                    Background = ShareResource.SelectedTheme.Brush.GetBrush();
                    break;
                case BackgroundType.Image:
                    BackgroundImage.Source = new BitmapImage(new Uri(ShareResource.SelectedTheme.File));
                    break;
                case BackgroundType.Vedio:
                    var player = new Windows.Media.Playback.MediaPlayer()
                    {
                        Source = MediaSource.CreateFromUri(new Uri(ShareResource.SelectedTheme.File))
                    };
                    player.CommandManager.IsEnabled = false;
                    player.IsLoopingEnabled = true;
                    player.Play();

                    MediaPlayerElement.SetMediaPlayer(player);
                    break;
                default:
                    break;
            }

            #region Color
            this.Resources["SystemAccentColor"] = ShareResource.SelectedTheme.ThemeColor;
            this.Resources["SystemAccentColorDark1"] = ShareResource.SelectedTheme.ThemeColor;
            this.Resources["SystemAccentColorDark2"] = ShareResource.SelectedTheme.ThemeColor;
            this.Resources["SystemAccentColorDark3"] = ShareResource.SelectedTheme.ThemeColor;
            this.Resources["SystemAccentColorLight1"] = ShareResource.SelectedTheme.ThemeColor;
            this.Resources["SystemAccentColorLight2"] = ShareResource.SelectedTheme.ThemeColor;
            this.Resources["SystemAccentColorLight3"] = ShareResource.SelectedTheme.ThemeColor;
            #endregion
        }

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

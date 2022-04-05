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

            ShareResource.SettingPage = this;
        }

        #region UI

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            _ = this.Dispatcher.RunAsync(default, delegate
            {
                LoadBackground();
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MediaPlayerElement.SetMediaPlayer(null);
            MediaPlayerElement.Source = null;
        }
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

        public void LoadBackground()
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

        public void Initialize()
        {
            ShareResource.InfoBar = InfoBar;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;

            if (!ShareResource.WebBrowserNavigateBack)
            {
                contentFrame.Navigate(typeof(BasicSettingsPage));
                MainNavigationViewItem.IsSelected = true;
            }
            else ShareResource.WebBrowserNavigateBack = false;

            UpdateInfoBadge();
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
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

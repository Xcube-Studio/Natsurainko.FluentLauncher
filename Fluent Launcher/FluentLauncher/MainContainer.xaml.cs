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
using Windows.Storage;
using Windows.Media.Playback;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainContainer : Page
    {
        public ThemeModel CurrentTheme { get; set; }

        public Frame ContentFrame { get; set; }

        public MainContainer()
        {
            ShareResource.DownloadNews = BeginDownloadNewsAsync();
            ShareResource.InitializeNewsPicture = ShareResource.BeginInitializeNewsPicture();
            this.InitializeComponent();

            _ = this.Dispatcher.RunAsync(default, delegate
            {
                contentFrame.Navigate(typeof(MainPage));

                LoadBackground();
            });

            ShareResource.InfoBar = InfoBar;
            ShareResource.MainContainer = this;
            ShareResource.MainContainer_TextBlock = AppTitle;
            ShareResource.AppTitleVisible = false;
            ContentFrame = contentFrame;
        }

        #region UI

        #region Page

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, args) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;
        }

        #endregion

        private async void LoadBackground()
        {
            if (CurrentTheme != null /*&& CurrentTheme.Equals(ShareResource.SelectedTheme)*/)
                return;

            CurrentTheme = ShareResource.SelectedTheme;

            RequestedTheme = ShareResource.SelectedTheme.ElementTheme;
            MediaPlayerElement.Visibility = Visibility.Collapsed;

            Background = new SolidColorBrush(Colors.Transparent);
            BackgroundImage.Source = null;

            switch (ShareResource.SelectedTheme.BackgroundType)
            {
                case BackgroundType.Normal:
                    Background = ShareResource.SelectedTheme.ElementTheme switch
                    {
                        ElementTheme.Dark => new SolidColorBrush(ColorConverter.FromString("#202020")),
                        _ => new SolidColorBrush(Colors.White),
                    };
                    break;
                case BackgroundType.Acrylic:
                    Background = ShareResource.SelectedTheme.Brush.GetBrush();
                    break;
                case BackgroundType.Image:
                    BackgroundImage.Source = new BitmapImage(new Uri(ShareResource.SelectedTheme.File));
                    break;
                case BackgroundType.Video:
                    MediaPlayerElement.SetMediaPlayer(new MediaPlayer());
                    MediaPlayerElement.Visibility = Visibility.Visible;

                    MediaPlayerElement.MediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(ShareResource.SelectedTheme.File));

                    MediaPlayerElement.MediaPlayer.CommandManager.IsEnabled = false;
                    MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;

                    MediaPlayerElement.MediaPlayer.Play();
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

        #endregion
    }
}

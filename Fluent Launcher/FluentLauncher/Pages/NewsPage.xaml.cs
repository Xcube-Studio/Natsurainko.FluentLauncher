using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewsPage : Page
    {
        public NewsPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Page
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ShareResource.InitializeNewsPicture;
            ContentListBox.ItemsSource = ShareResource.News.Entries;
            LoadingProgress.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Buttons
        private async void NavigateWebUrl(object sender, RoutedEventArgs e)
        {
            var req = new ValueSet() { { "Header", "NavigateWebUrl" }, { "Url", ((Button)sender).Tag.ToString() } };
            await App.DesktopBridge.Connection.SendMessageAsync(req);
        }
        #endregion

        #endregion

    }
}

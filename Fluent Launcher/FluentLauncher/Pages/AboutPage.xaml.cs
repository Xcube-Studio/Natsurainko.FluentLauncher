using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Buttons
        private void CloseButton(object sender, RoutedEventArgs e) => ContentDialog.Hide();
        #endregion

        #region HyperlinkButton
        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e) => await ContentDialog.ShowAsync();
        #endregion

        #endregion
    }
}

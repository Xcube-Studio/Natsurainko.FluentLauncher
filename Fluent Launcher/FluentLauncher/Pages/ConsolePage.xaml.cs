using FluentLauncher.Classes;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ConsolePage : Page
    {
        public ConsolePage()
        {
            this.InitializeComponent();
        }

        #region UI

        private bool EventRegister = false;

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TipBorder.Visibility = Visibility.Visible;
            InfoGrid.Visibility = Visibility.Collapsed;
            ContentListBox.ItemsSource = ShareResource.ProcessOutputs;

            if (ShareResource.MinecraftProcess.IsRunning && !EventRegister)
            {
                EventRegister = true;
                ShareResource.MinecraftProcess.Exited += Exited;
            }

            if (ShareResource.MinecraftProcess.IsRunning)
            {
                ShareResource.ProcessOutputs.CollectionChanged += ProcessOutputs_CollectionChanged;

                TipBorder.Visibility = Visibility.Collapsed;
                InfoGrid.Visibility = Visibility.Visible;

                UpdateInfoPanel();
                ScrollToEnd();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) => ShareResource.ProcessOutputs.CollectionChanged -= ProcessOutputs_CollectionChanged;

        #endregion

        #region HyperlinkButton
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(HomePage));
        #endregion

        private void ProcessOutputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => ScrollToEnd();

        private async void Exited(object sender, EventArgs e)
        {
            ShareResource.MinecraftProcess.Exited -= Exited;
            EventRegister = false;

            ShareResource.MinecraftProcess.McCore = string.Empty;
            ShareResource.MinecraftProcess.ProcessInfo = string.Empty;

            await this.Dispatcher.RunAsync(default, delegate
            {
                ContentListBox.SetItemsSource(ShareResource.ProcessOutputs);

                TipBorder.Visibility = Visibility.Visible;
                InfoGrid.Visibility = Visibility.Collapsed;

                UpdateInfoPanel();
            });
        }

        private void UpdateInfoPanel()
        {
            CoreBox.Text = ShareResource.MinecraftProcess.McCore;
            ProcessBox.Text = ShareResource.MinecraftProcess.ProcessInfo;
        }

        private void ScrollToEnd()
        {
            var border = VisualTreeHelper.GetChild(ContentListBox, 0) as Border;

            if (border.Child is ScrollViewer view)
            {
                try
                {

#pragma warning disable CS0618 // 类型或成员已过时
                    view.ScrollToVerticalOffset(view.ExtentHeight);
#pragma warning restore CS0618 // 类型或成员已过时

                    view.UpdateLayout();
                }
                catch { }
            }
        }

        #endregion
    }
}

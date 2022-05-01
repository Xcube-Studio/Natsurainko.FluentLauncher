using FluentLauncher.Classes;
using FluentLauncher.Models;
using System;
using System.Linq;
using System.Text;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        #region UI

        private bool NewsLoaded;

        #region Buttons
        private void ShowNewsBorder(object sender, RoutedEventArgs e)
        {
            NewsBorder.Visibility = Visibility.Visible;
            ShowNewsButton.Visibility = Visibility.Collapsed;

            ShareResource.HomePageNewsVisibility = true;
        }

        private void CloseNewsBorder(object sender, RoutedEventArgs e)
        {
            NewsBorder.Visibility = Visibility.Collapsed;
            ShowNewsButton.Visibility = Visibility.Visible;

            ShareResource.HomePageNewsVisibility = false;
        }

        private async void ReadMore(object sender, RoutedEventArgs e)
        {
            var req = new ValueSet() { { "Header", "NavigateWebUrl" }, { "Url", ((Button)sender).Tag.ToString() } };
            await App.DesktopBridge.Connection.SendMessageAsync(req);
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await ShareResource.SetSuitableJavaRuntime())
            {
                _ = ShareResource.ShowInfoAsync(ShareResource.LanguageResource.Background_SetSuitableJavaRuntime_False, string.Empty, 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
                return;
            }

            DownloadProgressText.Text = LaunchingProgressText.Text = string.Empty;

            ShareResource.MinecraftProcess.DownloadInfoReceived += MinecraftProcess_DownloadInfoReceived;
            ShareResource.MinecraftProcess.InfoReceived += MinecraftProcess_InfoReceived;
            ShareResource.MinecraftProcess.Exited += MinecraftProcess_Exited;
            ShareResource.MinecraftProcess.WaitForInputIdle += MinecraftProcess_WaitForInputIdle;
            ShareResource.MinecraftProcess.Crashed += MinecraftProcess_Crashed;
            ShareResource.MinecraftProcess.OutputReceived += MinecraftProcess_OutputReceived;
            ShareResource.MinecraftProcess.ErrorOutputReceived += MinecraftProcess_ErrorOutputReceived;

            ShareResource.MinecraftProcess.Launch();

            LaunchButton.IsEnabled = false;
            await LaunchDialog.ShowAsync();
        }

        private void CloseUpdateDialog(object sender, RoutedEventArgs e)
        {
            ShareResource.ShownUpdate = ShareResource.Version;
            UpdateDialog.Hide();
        }
        #endregion

        #region ComboBox
        private void LaunchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShareResource.SelectedCore = (MinecraftCoreInfo)LaunchComboBox.SelectedItem;
            UpdateLaunchButton();
        }

        #endregion

        #region Page
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShareResource.ShownUpdate != ShareResource.Version)
                _ = UpdateDialog.ShowAsync();

            AccountButton.DataContext = ShareResource.SelectedAccount;
            ShowNewsButton.Visibility = ShareResource.HomePageNewsVisibility ? Visibility.Collapsed : Visibility.Visible;
            NewsBorder.Visibility = ShareResource.HomePageNewsVisibility ? Visibility.Visible : Visibility.Collapsed;

            await ShareResource.UpdateMinecraftCoresAsync();
            UpdateComboBox();
            UpdateLaunchButton();

            HandleNavigateToLaunch();

            LaunchComboBox.SelectionChanged += LaunchComboBox_SelectionChanged;

            if (!NewsLoaded)
            {
                try
                {
                    await ShareResource.FirstNewsInitialized;

                    LoadingAnimation.Visibility = Visibility.Collapsed;
                    NewsBorder.DataContext = ShareResource.News.Entries[0];
                    NewsException.Visibility = ShareResource.News.Entries[0].BitmapImage == null ? Visibility.Visible : Visibility.Collapsed;
                    NewsLoaded = true;
                }
                catch { }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            LaunchComboBox.SelectionChanged -= LaunchComboBox_SelectionChanged;
        }
        #endregion

        #region MinecraftProcess

        private void MinecraftProcess_ErrorOutputReceived(object sender, ProcessOutput e)
        {
            _ = this.Dispatcher.RunAsync(default, () =>
            {
                ShareResource.ErrorProcessOutputs.Add(e);
            });
        }

        private void MinecraftProcess_OutputReceived(object sender, ProcessOutput e)
        {
            _ = this.Dispatcher.RunAsync(default, () =>
            {
                ShareResource.ProcessOutputs.Add(e);
            });
        }

        private async void MinecraftProcess_Crashed(object sender, EventArgs e)
        {
            ShareResource.MinecraftProcess.Crashed -= MinecraftProcess_Crashed;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Minecraft<{ShareResource.SelectedCore.Id}> Crashed");
            ShareResource.ErrorProcessOutputs.ForEach(e => { builder.AppendLine(e.Content); });
            ShareResource.ProcessOutputs.ToList().ForEach(e => { if (e.Type == "ERROR" || e.Type == "FATAL") builder.AppendLine(e.Content); });

            var jreInfo = await JavaHelper.GetInfo(ShareResource.SelectedJava);
            await ShareResource.ShowInfoAsync(builder.ToString(), $"OperatingSystem:{SystemRuntimeInfo.OperatingSystemInfo()}, " +
                $"JavaRuntime:{jreInfo.JAVA_VM_NAME} {jreInfo.JAVA_VERSION}", 45000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        }

        private async void MinecraftProcess_WaitForInputIdle(object sender, EventArgs e)
        {
            await this.Dispatcher.RunAsync(default, delegate
            {
                LaunchDialog.Hide();
            });

            ShareResource.MinecraftProcess.WaitForInputIdle -= MinecraftProcess_WaitForInputIdle;
            ShareResource.MinecraftProcess.DownloadInfoReceived -= MinecraftProcess_DownloadInfoReceived;
            ShareResource.MinecraftProcess.InfoReceived -= MinecraftProcess_InfoReceived;

            await this.Dispatcher.RunAsync(default, delegate
            {
                DownloadProgressText.Text = LaunchingProgressText.Text = string.Empty;

                this.Frame.Navigate(typeof(ConsolePage));
            });
        }

        private async void MinecraftProcess_Exited(object sender, EventArgs e)
        {
            ShareResource.MinecraftProcess.Exited -= MinecraftProcess_Exited;
            ShareResource.MinecraftProcess.OutputReceived -= MinecraftProcess_OutputReceived;
            ShareResource.MinecraftProcess.ErrorOutputReceived -= MinecraftProcess_ErrorOutputReceived;
            ShareResource.MinecraftProcess.IsRunning = false;

            await this.Dispatcher.RunAsync(default, delegate
            {
                ShareResource.ProcessOutputs.Clear();
                ShareResource.ErrorProcessOutputs.Clear();

                LaunchButton.IsEnabled = true;
            });
        }

        private void MinecraftProcess_InfoReceived(object sender, string e) => this.LaunchingProgressText.Text = e;

        private void MinecraftProcess_DownloadInfoReceived(object sender, string e) => this.DownloadProgressText.Text = e;

        #endregion

        private void HandleNavigateToLaunch()
        {
            if (ShareResource.NavigateToLaunch)
            {
                if (ShareResource.MinecraftProcess.IsRunning)
                {
                    _ = ShareResource.ShowInfoAsync("Failed to Launch", "There is already a running game");
                    ShareResource.NavigateToLaunch = false;
                    return;
                }
                LaunchButton_Click(null, null);
            }

            ShareResource.NavigateToLaunch = false;
        }

        private void UpdateComboBox()
        {
            LaunchComboBox.SetItemsSource(ShareResource.MinecraftCores);
            LaunchComboBox.SetSelectedItem(ShareResource.SelectedCore);
        }

        private void UpdateLaunchButton()
        {
            LaunchButtonTag.Text = ShareResource.SelectedCore != null ? ShareResource.SelectedCore.Id : ShareResource.LanguageResource.HomePage_LaunchButton_SubTitle;
            LaunchButton.IsEnabled = ShareResource.SelectedCore != null;

            if (ShareResource.MinecraftProcess.IsRunning)
                LaunchButton.IsEnabled = false;
        }
        #endregion
    }
}

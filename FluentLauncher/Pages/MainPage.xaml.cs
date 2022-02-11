using FluentLauncher.Classes;
using FluentLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

        #region UI

        private bool NewsLoaded;

        #region Buttons
        private void ShowNewsBorder(object sender, RoutedEventArgs e)
        {
            NewsBorder.Visibility = Visibility.Visible;
            ShowNewsButton.Visibility = Visibility.Collapsed;
        }

        private void CloseNewsBorder(object sender, RoutedEventArgs e)
        {
            NewsBorder.Visibility = Visibility.Collapsed;
            ShowNewsButton.Visibility = Visibility.Visible;
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

            await LaunchDialog.ShowAsync();
        }

        private void CloseUpdataDialog(object sender, RoutedEventArgs e)
        {
            ShareResource.ShownUpdata = ShareResource.Version;
            UpdataDialog.Hide();
        }
        #endregion

        #region ComboBox
        private void LaunchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            ShareResource.SelectedCore = (MinecraftCoreInfo)LaunchComboBox.SelectedItem;
            UpdataLaunchButton();
        }

        #endregion

        #region Page
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShareResource.ShownUpdata != ShareResource.Version)
                _ = UpdataDialog.ShowAsync();

            AccountButton.DataContext = ShareResource.SelectedAccount;

            await ShareResource.UpdataMinecraftCoresAsync();
            UpdataComboBox();
            UpdataLaunchButton();

            HandleNavigateToLaunch();

            LaunchComboBox.SelectionChanged += LaunchComboBox_SelectionChanged;

            if (!NewsLoaded)
            {
                await ShareResource.FirstNewsInitialized;
                LoadingAnimation.Visibility = Visibility.Collapsed;
                NewsBorder.DataContext = ShareResource.News.Entries[0];
                NewsException.Visibility = ShareResource.News.Entries[0].BitmapImage == null ? Visibility.Visible : Visibility.Collapsed;
                NewsLoaded = true;
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
            });
        }

        private void MinecraftProcess_InfoReceived(object sender, string e) => this.LaunchingProgressText.Text = e;

        private void MinecraftProcess_DownloadInfoReceived(object sender, string e) => this.DownloadProgressText.Text = e;

        #endregion

        private void HandleNavigateToLaunch()
        {
            if (ShareResource.NavigateToLaunch)
                LaunchButton_Click(null, null);

            ShareResource.NavigateToLaunch = false;
        }

        private void UpdataComboBox()
        {
            LaunchComboBox.SetItemsSource(ShareResource.MinecraftCores);
            LaunchComboBox.SetSelectedItem(ShareResource.SelectedCore);
        }

        private void UpdataLaunchButton()
        {
            LaunchButtonTag.Text = ShareResource.SelectedCore != null ? ShareResource.SelectedCore.Id : ShareResource.LanguageResource.MainPage_LaunchButton_SubTitle;
            LaunchButton.IsEnabled = ShareResource.SelectedCore != null;
        }
        #endregion
    }
}

using FluentLauncher.Classes;
using FluentLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CoresPage : Page
    {
        public CoresPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Buttons
        private async void ShowDialogButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShareResource.DownloadVersionManifest == null)
                ShareResource.DownloadVersionManifest = ShareResource.BeginDownloadVersionManifest();

            LoadingDialog();
            await InstallContentDialog.ShowAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => InstallContentDialog.Hide();

        private void CancelButton_Click_1(object sender, RoutedEventArgs e) => RenameContentDialog.Hide();

        private async void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var core = (MinecraftCoreInfo)((Button)sender).DataContext;
            await App.DesktopBridge.SendAsync<StandardResponseModel>
                (new RenameMinecraftCoreRequest(core.Id, RenameAutoSuggestBox.Text));

            await ShareResource.UpdateMinecraftCoresAsync();
            UpdateListBox();
            RenameContentDialog.Hide();
        }

        private void PlayButton(object sender, RoutedEventArgs e)
        {
            ShareResource.SelectedCore = (MinecraftCoreInfo)((Button)sender).DataContext;
            ShareResource.NavigateToLaunch = true;
            this.Frame.Navigate(typeof(MainPage));
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CoreOptionsPage));
        }

        private async void InstallMinecraft(object sender, RoutedEventArgs e)
        {
            await BeginInstallAsync();

            InstallContentDialog.Hide();
        }
        #endregion

        #region Page
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = ((Grid)ShowDialogButton.Parent);

            if (ShareResource.SelectedFolder == null || ShareResource.MinecraftFolders.Count == 0)
            {
                ToolTipService.SetToolTip(grid, "You need to choose at least one .minecraft folder");
                ShowDialogButton.IsEnabled = false;
                TipGrid.Visibility = Visibility.Visible;
                ContentGird.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToolTipService.SetToolTip(grid, null);
                ShowDialogButton.IsEnabled = true;
                TipGrid.Visibility = Visibility.Collapsed;
                ContentGird.Visibility = Visibility.Visible;
            }

            await ShareResource.UpdateMinecraftCoresAsync();
            UpdateListBox();

            ContentListBox.SelectionChanged += ContentListBox_SelectionChanged;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentListBox.SelectionChanged -= ContentListBox_SelectionChanged;
        }
        #endregion

        #region Border
        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var control = (Border)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Visible;
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var control = (Border)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region ListBox
        private void ContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ShareResource.SelectedCore = (MinecraftCoreInfo)ContentListBox.SelectedItem;
        #endregion

        #region MenuFlyoutItem
        private void LaunchMenuFlyoutItem(object sender, RoutedEventArgs e)
        {
            ShareResource.SelectedCore = (MinecraftCoreInfo)((MenuFlyoutItem)sender).DataContext;
            ShareResource.NavigateToLaunch = true;
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void FolderMenuFlyoutItem(object sender, RoutedEventArgs e)
        {
            var core = (MinecraftCoreInfo)((MenuFlyoutItem)sender).DataContext;
            await App.DesktopBridge.SendAsync<StandardResponseModel>
                (new StandardRequestModel() { Header = "NavigateFolder", Message = ShareResource.SelectedFolder.Path });
        }

        private async void DeleteMenuFlyoutItem(object sender, RoutedEventArgs e)
        {
            var core = (MinecraftCoreInfo)((MenuFlyoutItem)sender).DataContext;

            await App.DesktopBridge.SendAsync<StandardResponseModel>
                (new DeleteMinecraftCoreRequest() { Folder = ShareResource.SelectedFolder.Path, Name = core.Id });

            await ShareResource.UpdateMinecraftCoresAsync();
            UpdateListBox();
        }

        private async void RenameMenuFlyoutItem(object sender, RoutedEventArgs e)
        {
            RenameButton.IsEnabled = false;
            RenameButton.DataContext = ((MenuFlyoutItem)sender).DataContext;
            RenameAutoSuggestBox.Text = string.Empty;
            await RenameContentDialog.ShowAsync();
        }

        private async void MakeScriptMenuFlyoutItem(object sender, RoutedEventArgs e)
        {
            var core = (MinecraftCoreInfo)((MenuFlyoutItem)sender).DataContext;
            var req = new LaunchMinecraftRequest();
            var builder = new StringBuilder();

            req.Header = "GetLaunchArguments";
            req.Id = core.Id;

            var res = await App.DesktopBridge.SendAsync<StandardResponseModel>(req);

            if (!await ShareResource.SetSuitableJavaRuntime())
            {
                _ = ShareResource.ShowInfoAsync(ShareResource.LanguageResource.Background_SetSuitableJavaRuntime_False, string.Empty, 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
                return;
            }

            builder.AppendLine("@echo off");
            builder.AppendLine($"set APPDATA={new DirectoryInfo(ShareResource.SelectedFolder.Path).Parent.FullName}");
            builder.AppendLine($"cd /D {(req.IsIndependent ? Path.Combine(req.GameFolder, "versions", req.Id) : ShareResource.SelectedFolder.Path)}");
            builder.AppendLine(res.Response);
            builder.AppendLine("pause");
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "LaunchMinecraft"
            };
            savePicker.FileTypeChoices.Add("批处理文件", new List<string>() { ".bat" });

            var file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                await FileIO.WriteTextAsync(file, builder.ToString());
                _ = ShareResource.ShowInfoAsync(ShareResource.LanguageResource.Background_MakeScript_Successfully, string.Empty, 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
            }
        }
        #endregion

        #region ComboBox
        private void VersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModLoaderComboBox.SelectedIndex = -1;
            InstallButton.IsEnabled = true;

            if (VersionComboBox.SelectedItem == null)
            {
                InstallButton.IsEnabled = false;
                ModLoaderToggleSwitch.IsEnabled = false;
                ModLoaderToggleSwitch.IsOn = false;

                UpdateModLoaderUI();
                return;
            }

            if (((VersionManifestItem)VersionComboBox.SelectedItem).Type != "release")
            {
                ModLoaderToggleSwitch.IsOn = false;
                ModLoaderToggleSwitch.IsEnabled = false;
            }
            else
            {
                ModLoaderToggleSwitch.IsEnabled = true;
                CheckModLoader();
            }

            UpdateModLoaderUI();
        }

        private void ModLoaderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateModLoaderUI();
        #endregion

        #region ToggleSwitch
        private void ModLoaderToggleSwitch_Toggled(object sender, RoutedEventArgs e) => UpdateModLoaderUI();
        #endregion

        #region TextBox
        private void RenameAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(RenameAutoSuggestBox.Text))
                return;

            bool isRepeat = false;
            foreach (var item in ShareResource.MinecraftCores)
                if (item.Id == RenameAutoSuggestBox.Text)
                    isRepeat = true;

            RenameButton.IsEnabled = !isRepeat;
        }
        #endregion

        #region HyperlinkButton
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) => ((Frame)Window.Current.Content).Navigate(typeof(SettingPage));
        #endregion

        private async Task BeginInstallAsync()
        {
            if (ModLoaderComboBox.SelectedItem != null && ShareResource.SelectedJava == null)
            {
                _ = ShareResource.ShowInfoAsync($"Failed to Install Minecraft {McInfoTextBlock.Text}", "You need at least one Java runtime to install the Mod loader", 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
                return;
            }

            VersionComboBox.IsEnabled = ModLoaderComboBox.IsEnabled = CancelButton.IsEnabled = InstallButton.IsEnabled = false;
            InstallProgressPanel.Visibility = Visibility.Visible;

            await ShareResource.SetDownloadSource();

            async void Connection_RequestReceived(Windows.ApplicationModel.AppService.AppServiceConnection sender, Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs args)
            {
                if ((string)args.Request.Message["Header"] == "InstallMinecraftProgress")
                {
                    var model = JsonConvert.DeserializeObject<InstallMinecraftProgressResponse>(JsonConvert.SerializeObject(args.Request.Message));
                    await this.Dispatcher.RunAsync(default, delegate
                    {
                        InstallProgressText.Text = model.Message;
                        InstallProgressBar.Value = model.Progress * 100;
                        InstallProgressBar.IsIndeterminate = false;
                    });
                }
            }

            App.DesktopBridge.Connection.RequestReceived += Connection_RequestReceived;
            var task = App.DesktopBridge.SendAsync<StandardResponseModel>(new InstallMinecraftRequest
            {
                Folder = ShareResource.SelectedFolder.Path,
                JavaPath = ShareResource.SelectedJava?.Path,
                McVersion = ((VersionManifestItem)VersionComboBox.SelectedItem).Id,
                ModLoader = JsonConvert.SerializeObject(((AbstractModLoader)ModLoaderComboBox.SelectedItem))
            });

            var res = await task;
            InstallProgressPanel.Visibility = Visibility.Collapsed;
            CancelButton.IsEnabled = true;

            App.DesktopBridge.Connection.RequestReceived -= Connection_RequestReceived;

            if (Convert.ToBoolean(res.Message))
            {
                _ = ShareResource.ShowInfoAsync($"Installed Minecraft {McInfoTextBlock.Text} Successfully", string.Empty, 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
                await ShareResource.UpdateMinecraftCoresAsync();
                UpdateListBox();
            }
            else _ = ShareResource.ShowInfoAsync($"Failed to Install Minecraft {McInfoTextBlock.Text}", "There is something wrong..?", 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        }

        private void UpdateModLoaderUI()
        {
            string info = string.Empty;

            switch (ModLoaderToggleSwitch.IsOn)
            {
                case true:
                    if (ModLoaderComboBox.Items.Count == 0)
                        ModLoaderComboBox.IsEnabled = false;
                    else ModLoaderComboBox.IsEnabled = true;
                    break;
                case false:
                    ModLoaderComboBox.IsEnabled = false;
                    break;
            }

            if (VersionComboBox.SelectedItem != null)
            {
                var item = (VersionManifestItem)VersionComboBox.SelectedItem;
                info += item.Id;
            }

            if (ModLoaderToggleSwitch.IsOn && ModLoaderComboBox.SelectedItem != null)
            {
                var item = (AbstractModLoader)ModLoaderComboBox.SelectedItem;
                info += $"-{item.Type}";
            }

            McInfoTextBlock.Text = info;
        }

        private async void CheckModLoader()
        {
            string mcVersion = ((VersionManifestItem)VersionComboBox.SelectedItem).Id;
            var list = new List<AbstractModLoader>();

            if (ShareResource.ForgeSupportMcVersionList.Contains(mcVersion))
            {
                var build = await ShareResource.GetLatestForgeVersion(mcVersion);
                list.Add(new AbstractModLoader
                {
                    Build = build,
                    LoaderVersion = build.Version,
                    McVersion = mcVersion,
                    Type = "Forge"
                });
            }
            if (ShareResource.OptiFineSupportMcVersionList.Contains(mcVersion))
            {
                var build = await ShareResource.GetLatestOptiFineVersion(mcVersion);
                list.Add(new AbstractModLoader
                {
                    Build = build,
                    LoaderVersion = $"{build.Type}_{build.Patch}",
                    McVersion = mcVersion,
                    Type = "OptiFine"
                });
            }

            ModLoaderComboBox.ItemsSource = list;
        }

        private async void LoadingDialog()
        {
            await ShareResource.DownloadVersionManifest;

            if (ShareResource.ForgeSupportMcVersionList == null)
                ShareResource.ForgeSupportMcVersionList = await ShareResource.GetForgeSupportMcVersionList();

            if (ShareResource.OptiFineSupportMcVersionList == null)
                ShareResource.OptiFineSupportMcVersionList = ShareResource.GetOptiFineSupportMcVersionList();

            InstallProgressBar.Value = 0;
            InstallProgressBar.IsIndeterminate = true;
            InstallProgressText.Text = "";

            VersionComboBox.ItemsSource = ShareResource.VersionManifest.Versions.ToList();
            VersionComboBox.SelectedIndex = 0;
            VersionComboBox.IsEnabled = true;

            InstallContentDialogContentGrid.Opacity = 100;
            LoadingGrid.Visibility = Visibility.Collapsed;
        }

        private void UpdateListBox()
        {
            ContentListBox.SetItemsSource(ShareResource.MinecraftCores);
            ContentListBox.SetSelectedItem(ShareResource.SelectedCore);

            if (ShareResource.SelectedCore != null)
            {
                ContentListBox.ScrollIntoView(ShareResource.SelectedCore);
                ContentListBox.UpdateLayout();
            }
        }

        #endregion

    }
}

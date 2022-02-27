using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FluentLauncher.Models;
using FluentLauncher.Classes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Web.Http.Diagnostics;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BasicSettingsPage : Page
    {
        public BasicSettingsPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Grid
        private void Item_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var control = (Grid)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Visible;
        }

        private void Item_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var control = (Grid)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Button
        private void CancelAddFolder(object sender, RoutedEventArgs e) => AddFolderDialog.Hide();

        private void CancelAddJava(object sender, RoutedEventArgs e) => AddJavaDialog.Hide();

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            var newItem = new MinecraftFolder()
            {
                Path = AddFolderPathBox.Text,
                Title = AddFolderNameBox.Text
            };
            ShareResource.MinecraftFolders.AddWithUpdate(newItem);
            UpdateListBox("Folder");
            ShareResource.SelectedFolder = newItem;
            UpdateListBox("Folder");
            this.AddFolderDialog.Hide();
        }

        private void AddJava(object sender, RoutedEventArgs e)
        {
            var newItem = new JavaRuntimeEnvironment()
            {
                Path = AddJavaPathBox.Visibility == Visibility.Visible ? AddJavaPathBox.Text : ((JavaRuntimeEnvironment)SearchJavaPathComboBox.SelectedItem).Path,
                Title = AddJavaNameBox.Text
            };
            ShareResource.JavaRuntimeEnvironments.AddWithUpdate(newItem);
            ShareResource.SelectedJava = newItem;
            UpdateListBox("Java");
            ShareResource.SelectedJava = newItem;
            UpdateListBox("Java");
            this.AddJavaDialog.Hide();
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            var button = ((Button)sender);
            switch ((string)button.Tag)
            {
                case "Folder":
                    ShareResource.MinecraftFolders.Remove((MinecraftFolder)button.DataContext);
                    App.Settings.Values["MinecraftFolders"] = JsonConvert.SerializeObject(ShareResource.MinecraftFolders);
                    if (!ShareResource.MinecraftFolders.Contains(ShareResource.SelectedFolder))
                        ShareResource.SelectedFolder = null;
                    UpdateListBox("Folder");
                    _ = ShareResource.UpdateMinecraftCoresAsync();
                    break;
                case "Java":
                    ShareResource.JavaRuntimeEnvironments.Remove((JavaRuntimeEnvironment)button.DataContext);
                    App.Settings.Values["JavaRuntimeEnvironments"] = JsonConvert.SerializeObject(ShareResource.JavaRuntimeEnvironments);
                    if (!ShareResource.JavaRuntimeEnvironments.Contains(ShareResource.SelectedJava))
                        ShareResource.SelectedJava = null;
                    UpdateListBox("Java");
                    break;
                default:
                    break;
            }
        }

        private async void SearchJavaButton_Click(object sender, RoutedEventArgs e)
        {
            var res = await App.DesktopBridge.SendAsync<StandardResponseModel>(new StandardRequestModel()
            {
                Header = "SearchJavaRuntime"
            });

            var list = JsonConvert.DeserializeObject<List<JavaRuntimeEnvironment>>(res.Response);
            if (list.Count == 0)
            {
                AddJavaDialog.Hide();
                _ = ShareResource.ShowInfoAsync(ShareResource.LanguageResource.Background_SearchJava_Error, string.Empty, 0, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
            }
            else
            {
                SearchJavaPathComboBox.Visibility = Visibility.Visible;
                AddJavaPathBox.Visibility = Visibility.Collapsed;

                SearchJavaPathComboBox.SetItemsSource(list);
                SearchJavaPathComboBox.SelectedIndex = 0;
            }
        }

        private async void InstallJavaButton_Click(object sender, RoutedEventArgs e)
        {
            AddJavaDialog.Hide();
            _ = InstallJavaDialog.ShowAsync();

            InstallProgressTitle.Text = string.Empty;
            InstallProgressSubTitle.Content = string.Empty;
            InstallJavaProgressBar.IsIndeterminate = true;
            InstallJavaProgressBar.Value = 0;
            DownloadProgress.Text = "0 Mb/0 Mb";

            var task = App.DesktopBridge.SendAsync<StandardResponseModel>(new StandardRequestModel()
            {
                Header = "InstallJavaRuntime"
            });

            async void Connection_RequestReceived(Windows.ApplicationModel.AppService.AppServiceConnection sender, Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs args)
            {
                if ((string)args.Request.Message["Header"] == "InstallJavaProgress")
                    await this.Dispatcher.RunAsync(default, delegate
                    {
                        switch ((string)args.Request.Message["Response"])
                        {
                            case "Progress_Title":
                                InstallProgressTitle.Text = (string)args.Request.Message["Message"];

                                if ((string)args.Request.Message["Message"] == "Extracting files..")
                                    InstallJavaProgressBar.IsIndeterminate = true;
                                break;
                            case "Progress_SubTitle":
                                InstallProgressSubTitle.Content = (string)args.Request.Message["Message"];
                                break;
                            case "ProgressBar_Value":
                                if (InstallJavaProgressBar.IsIndeterminate)
                                    InstallJavaProgressBar.IsIndeterminate = false;

                                InstallJavaProgressBar.Value = Convert.ToDouble((string)args.Request.Message["Message"]);
                                break;
                            case "Progress_Text":
                                DownloadProgress.Text = (string)args.Request.Message["Message"];
                                break;
                            default:
                                break;
                        }
                    });
            }
            App.DesktopBridge.Connection.RequestReceived += Connection_RequestReceived;

            var res = await task;
            App.DesktopBridge.Connection.RequestReceived -= Connection_RequestReceived;
            InstallJavaDialog.Hide();

            if (Convert.ToBoolean(res.Message))
            {
                var info = await JavaHelper.GetInfo(res.Response);

                var newItem = new JavaRuntimeEnvironment()
                {
                    Path = res.Response,
                    Title = $"{info.JAVA_VM_NAME} {info.JAVA_VERSION}"
                };
                ShareResource.JavaRuntimeEnvironments.AddWithUpdate(newItem);
                ShareResource.SelectedJava = newItem;
                UpdateListBox("Java");

                _ = ShareResource.ShowInfoAsync("Install Java Runtime Successfully", string.Empty, 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
            }
            else _ = ShareResource.ShowInfoAsync("Failed to Install Java Runtime", string.Empty, 3000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        }
        #endregion

        #region InputBox
        private async void AddFolderPathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
                AddFolderPathBox.Text = folder.Path;
        }

        private async void AddJavaPathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".exe");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                AddJavaPathBox.Text = file.Path;

                if (file.Name == "javaw.exe")
                {
                    var res = await JavaHelper.GetInfo(file.Path);
                    AddJavaNameBox.Text = $"{res.JAVA_VM_NAME} {res.JAVA_VERSION}";
                }
            }
        }

        private void MaxMemoryBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
            => ShareResource.MaxMemory = (int)args.NewValue;

        private void MinMemoryBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
            => ShareResource.MinMemory = (int)args.NewValue;

        private async void AddFolderPathBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => AddFolderButton.IsEnabled = await IOHelper.FolderExist(AddFolderPathBox.Text);

        private async void AddJavaPathBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => AddJavaButton.IsEnabled = await IOHelper.FileExist(AddJavaPathBox.Text);

        #endregion

        #region ComboBox
        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ShareResource.WorkingFolder = (string)ModeComboBox.SelectedItem;
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ShareResource.Language = (string)LanguageComboBox.SelectedItem;

        private void SearchJavaPathComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchJavaPathComboBox.SelectedItem == null && SearchJavaPathComboBox.Visibility != Visibility.Visible)
                return;

            var java = (JavaRuntimeEnvironment)SearchJavaPathComboBox.SelectedItem;
            AddJavaNameBox.Text = java.Title;
        }
        #endregion

        #region ListViewItem
        private async void AddFolder_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            AddFolderNameBox.Text = "New Minecraft Folder";
            AddFolderPathBox.Text = string.Empty;
            AddFolderButton.IsEnabled = false;

            await AddFolderDialog.ShowAsync();
        }

        private async void AddJava_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SearchJavaPathComboBox.Visibility = Visibility.Collapsed;
            AddJavaPathBox.Visibility = Visibility.Visible;
            SearchJavaPathComboBox.SetItemsSource(null);
            SearchJavaPathComboBox.SelectedIndex = -1;

            AddJavaNameBox.Text = "Java(TM) Platform SE Binary";
            AddJavaPathBox.Text = string.Empty;
            AddJavaButton.IsEnabled = false;

            await AddJavaDialog.ShowAsync();
        }
        #endregion

        #region ListView
        private void FoldersListView_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => ShareResource.SelectedFolder = (MinecraftFolder)FoldersListView.SelectedItem;

        private async void JavasListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShareResource.SelectedJava = (JavaRuntimeEnvironment)JavasListView.SelectedItem;

            if (JavasListView.SelectedItem != null)
            {
                JreInfoStackPanel.Visibility = Visibility.Visible;
                JreInfoStackPanel.DataContext = await JavaHelper.GetInfo(ShareResource.SelectedJava);
            }
            else JreInfoStackPanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Page
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateListBox("Folder");
            UpdateListBox("Java");
            MaxMemoryBox.Value = ShareResource.MaxMemory;
            MinMemoryBox.Value = ShareResource.MinMemory;
            ModeComboBox.SetItemsSource(ShareResource.WorkingFolders);
            ModeComboBox.SetSelectedItem(ShareResource.WorkingFolder);
            LanguageComboBox.SetItemsSource(ShareResource.Languages);
            LanguageComboBox.SetSelectedItem(ShareResource.Language);

            if (JavasListView.SelectedItem != null)
            {
                JreInfoStackPanel.Visibility = Visibility.Visible;
                JreInfoStackPanel.DataContext = await JavaHelper.GetInfo(ShareResource.SelectedJava);
            }
            else JreInfoStackPanel.Visibility = Visibility.Collapsed;

            FoldersListView.SelectionChanged += FoldersListView_SelectionChanged;
            JavasListView.SelectionChanged += JavasListView_SelectionChanged;
            ModeComboBox.SelectionChanged += ModeComboBox_SelectionChanged;
            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            SearchJavaPathComboBox.SelectionChanged += SearchJavaPathComboBox_SelectionChanged;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            FoldersListView.SelectionChanged -= FoldersListView_SelectionChanged;
            JavasListView.SelectionChanged -= JavasListView_SelectionChanged;
            ModeComboBox.SelectionChanged -= ModeComboBox_SelectionChanged;
            LanguageComboBox.SelectionChanged -= LanguageComboBox_SelectionChanged;
            SearchJavaPathComboBox.SelectionChanged -= SearchJavaPathComboBox_SelectionChanged;
        }
        #endregion

        private void UpdateListBox(string name)
        {
            switch (name)
            {
                case "Folder":
                    FoldersListView.SetItemsSource(ShareResource.MinecraftFolders);
                    FoldersListView.SetSelectedItem(ShareResource.SelectedFolder);
                    break;
                case "Java":
                    JavasListView.SetItemsSource(ShareResource.JavaRuntimeEnvironments);
                    JavasListView.SetSelectedItem(ShareResource.SelectedJava);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}

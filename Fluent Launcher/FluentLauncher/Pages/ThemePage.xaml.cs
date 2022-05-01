using FluentLauncher.Classes;
using FluentLauncher.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing3D;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThemePage : Page
    {
        public ThemePage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Border
        private void Item_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var control = (Border)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Visible;
        }

        private void Item_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var control = (Border)sender;
            var panel = (StackPanel)control.FindName("Tools");
            panel.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGridView();

            ContentGridView.SelectionChanged += ContentGridView_SelectionChanged;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentGridView.SelectionChanged -= ContentGridView_SelectionChanged;
        }
        #endregion

        #region GridView
        private async void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ContentGridView.SelectedItem;

            if (item == null)
                return;

            if (item.GetType() == typeof(GridViewItem))
            {
                LoadDialog();
                await AddDialog.ShowAsync();
                return;
            }

            ShareResource.SelectedTheme = (ThemeModel)item;

            ChangeTip.IsOpen = true;
            //ShareResource.SettingPage.LoadBackground();
            //App.UpdateAppTheme();
        }
        #endregion

        #region Buttons
        private void Cancel_Click(object sender, RoutedEventArgs e) => AddDialog.Hide();

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            BackgroundType backgroundType = default;
            ElementTheme color = default;

            switch ((string)BackgroundTypeBox.SelectedValue)
            {
                case "Normal":
                case "纯色":
                    backgroundType = BackgroundType.Normal;
                    break;
                case "Acrylic":
                case "亚克力":
                    backgroundType = BackgroundType.Acrylic;
                    break;
                case "Image":
                case "图片":
                    backgroundType = BackgroundType.Image;
                    break;
                case "Video":
                case "视频":
                    backgroundType = BackgroundType.Video;
                    break;
                default:
                    break;
            }
            switch ((string)ColorModeBox.SelectedValue)
            {
                case "Dark":
                case "深色":
                    color = ElementTheme.Dark;
                    break;
                case "Light":
                case "浅色":
                    color = ElementTheme.Light;
                    break;
                default:
                    break;
            }

            var brushType = backgroundType == BackgroundType.Acrylic ? BrushType.Acyrlic : BrushType.Solid;

            var newItem = new ThemeModel
            {
                BackgroundType = backgroundType,
                Brush = new UniversalBrush
                {
                    TintLuminosityOpacity = TintLuminosityOpacityBox.Value,
                    TintOpacity = TintOpacityBox.Value,
                    BrushType = brushType,
                    Color = color == ElementTheme.Dark ? Colors.Black : Colors.White
                },
                ElementTheme = color,
                Name = NameBox.Text,
                ThemeColor = ColorPicker.Color
            };

            if (!string.IsNullOrEmpty(FilePathBox.Text) && await IOHelper.FileExist(FilePathBox.Text))
            {
                var file = new FileInfo(FilePathBox.Text);
                string filename = Path.Combine(ShareResource.StorageFolder, "Images", Guid.NewGuid().ToString("N"), file.Extension);
                await IOHelper.CopyToFolder(file.FullName, filename);
                newItem.File = filename;
            }

            ShareResource.Themes.AddWithUpdate(newItem);
            UpdateGridView();

            AddDialog.Hide();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var model = (ThemeModel)button.DataContext;

            if (await IOHelper.FileExist(model.File))
            {
                var file = new FileInfo(model.File);
                file.Delete();
                file.Directory.Delete();
            }

            ShareResource.Themes.Remove(model);
            App.Settings.Values["Themes"] = JsonConvert.SerializeObject(ShareResource.Themes);

            if (!ShareResource.Themes.Contains(ShareResource.SelectedTheme))
                ShareResource.SelectedTheme = ShareResource.Themes[0];

            UpdateGridView();
        }

        private async void FilePathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".mp4");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                FilePathBox.Text = file.Path;
            }

        }
        #endregion

        #region ComboBox
        private void BackgroundTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string)BackgroundTypeBox.SelectedValue)
            {
                case "Normal":
                case "纯色":
                    FilePathBox.Visibility = Visibility.Collapsed;
                    AcrylicBrushArugmentsGrid.Visibility = Visibility.Collapsed;
                    break;
                case "Acrylic":
                case "亚克力":
                    FilePathBox.Visibility = Visibility.Collapsed;
                    AcrylicBrushArugmentsGrid.Visibility = Visibility.Visible;
                    break;
                case "Image":
                case "图片":
                case "Video":
                case "视频":
                    FilePathBox.Visibility = Visibility.Visible;
                    AcrylicBrushArugmentsGrid.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void UpdateGridView()
        {
            var list = new List<object>() { (ContentGridView.Items)[0] };

            foreach (var item in ShareResource.Themes)
                list = list.Append(item).ToList();

            ContentGridView.SetItemsSource(list);
            ContentGridView.SetSelectedItem((object)ShareResource.SelectedTheme);
        }

        private void LoadDialog()
        {
            TintLuminosityOpacityBox.Value = 0.0;
            TintOpacityBox.Value = 0.0;

            FilePathBox.Text = string.Empty;
            BackgroundTypeBox.ItemsSource = ShareResource.BackgroundTypes;
            BackgroundTypeBox.SelectedItem = 0;

            ColorModeBox.ItemsSource = ShareResource.ColorModes;
            ColorModeBox.SelectedItem = 0;

            ColorPicker.Color = ShareResource.SelectedTheme.ThemeColor;
            NameBox.Text = string.Empty;
        }

        #endregion
    }
}

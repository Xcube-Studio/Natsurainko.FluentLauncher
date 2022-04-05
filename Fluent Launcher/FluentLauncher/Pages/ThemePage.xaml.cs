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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
        private void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ContentGridView.SelectedItem;

            if (item == null || item.GetType() == typeof(GridViewItem))
                return;

            ShareResource.SelectedTheme = (ThemeModel)item;

            ShareResource.SettingPage.LoadBackground();
            App.UpdateAppTheme();
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

        #endregion
    }
}

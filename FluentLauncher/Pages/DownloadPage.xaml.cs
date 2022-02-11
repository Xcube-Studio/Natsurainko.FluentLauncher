using FluentLauncher.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class DownloadPage : Page
    {
        public DownloadPage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentComboBox.ItemsSource = ShareResource.DownloadSources;
            ContentComboBox.SetSelectedItem(ShareResource.DownloadSource);
            ThreadsSlider.Value = ShareResource.MaxThreads;

            ContentComboBox.SelectionChanged += ContentComboBox_SelectionChanged;
            ThreadsSlider.ValueChanged += ThreadsSlider_ValueChanged;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentComboBox.SelectionChanged -= ContentComboBox_SelectionChanged;
            ThreadsSlider.ValueChanged -= ThreadsSlider_ValueChanged;
        }
        #endregion

        #region ComboBox
        private void ContentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ShareResource.DownloadSource = (string)ContentComboBox.SelectedItem;
        #endregion

        #region NumberBox
        private void NumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
            => ShareResource.MaxThreads = (int)args.NewValue;
        #endregion

        #region Slider
        private void ThreadsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
            => ShareResource.MaxThreads = (int)e.NewValue;
        #endregion

        #endregion
    }
}

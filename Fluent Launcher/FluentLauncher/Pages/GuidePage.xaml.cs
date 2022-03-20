using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentLauncher.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GuidePage : Page
    {
        public GuidePage()
        {
            this.InitializeComponent();
        }

        #region UI

        #region Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShareResource.RunForFirstTime = false;

            this.Frame.Navigate(typeof(MainContainer));

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) => Application.Current.Exit();
        #endregion

        #region Page
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, args) => UpdateAppTitle(s);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = ((SolidColorBrush)AppTitle.Foreground).Color;
        }
        #endregion

        #region ScrollViewer
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer viewer = (ScrollViewer)sender;
            if (viewer.VerticalOffset >= viewer.ExtentHeight - viewer.ViewportHeight)
                CheckBox.IsEnabled = true;
        }
        #endregion

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        #endregion

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewer_ViewChanged(ScrollViewer, null);
        }
    }
}

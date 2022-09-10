using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Pages.Activities;
using Richasy.ExpanderEx.Uwp;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Natsurainko.FluentLauncher.View.Pages.Activities;

public sealed partial class ActivityLaunchPage : Page
{
    public ActivityLaunchPageVM ViewModel { get; set; }

    public ActivityLaunchPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<ActivityLaunchPageVM, Page>(this);
    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e) => MainContainer.ContentFrame.Navigate(typeof(HomePage));

    private void ExpanderEx_PointerEntered(object sender, PointerRoutedEventArgs e)
        => ((Button)((ExpanderEx)sender).FindName("CloseButton")).Visibility = Visibility.Visible;

    private void ExpanderEx_PointerExited(object sender, PointerRoutedEventArgs e)
        => ((Button)((ExpanderEx)sender).FindName("CloseButton")).Visibility = Visibility.Collapsed;

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        var launcherProcess = (LauncherProcessViewData)((Button)sender).DataContext;

        CacheResources.LauncherProcesses.Remove(launcherProcess);
        ViewModel.LauncherProcesses.Remove(launcherProcess);
    }

    private async void StopButton_Click(object sender, RoutedEventArgs e)
    {
        var launcherProcess = (LauncherProcessViewData)((Button)sender).DataContext;
        await launcherProcess.Data.StopProcessAsync();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var launcherProcess = (LauncherProcessViewData)((Button)sender).DataContext;

        var dataPackage = new DataPackage();
        dataPackage.SetText(string.Join(' ', launcherProcess.Data.Arguments));

        Clipboard.SetContent(dataPackage);

        MainContainer.ShowInfoBarAsync("成功复制启动参数", severity: Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
    }

    private async void ShowLogButton_Click(object sender, RoutedEventArgs e)
    {
        await ((LauncherProcessViewData)((Button)sender).DataContext).CreateProcessOutputWindow();
    }
}

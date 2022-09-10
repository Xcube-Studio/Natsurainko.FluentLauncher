using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.View.Pages.Resources;
using Natsurainko.FluentLauncher.ViewModel.Pages.Activities;
using Richasy.ExpanderEx.Uwp;
using System;
using System.IO;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Natsurainko.FluentLauncher.View.Pages.Activities;

public sealed partial class ActivityDownloadPage : Page
{
    public ActivityDownloadPageVM ViewModel { get; private set; }

    public ActivityDownloadPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<ActivityDownloadPageVM, Page>(this);
    }

    private void ExpanderEx_PointerEntered(object sender, PointerRoutedEventArgs e)
        => ((Button)((ExpanderEx)sender).FindName("CloseButton")).Visibility = Visibility.Visible;

    private void ExpanderEx_PointerExited(object sender, PointerRoutedEventArgs e)
        => ((Button)((ExpanderEx)sender).FindName("CloseButton")).Visibility = Visibility.Collapsed;

    private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        => await Launcher.LaunchFolderPathAsync(new FileInfo((string)((Button)sender).Tag).Directory.FullName);

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        => MainContainer.ContentFrame.Navigate(typeof(ResourcesPage));

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        var downloaderProcess = (DownloaderProcessViewData)((Button)sender).DataContext;

        CacheResources.DownloaderProcesses.Remove(downloaderProcess);
        ViewModel.DownloaderProcesses.Remove(downloaderProcess);
    }
}

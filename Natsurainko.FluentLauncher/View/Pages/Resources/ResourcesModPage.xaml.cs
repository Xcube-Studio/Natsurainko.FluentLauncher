using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Pages.Resources;
using Richasy.ExpanderEx.Uwp;
using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Natsurainko.FluentLauncher.View.Pages.Resources;

public sealed partial class ResourcesModPage : Page
{
    public ResourcesModPageVM ViewModel { get; set; }

    public ResourcesModPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<ResourcesModPageVM, Page>(this);
    }

    private void ExpanderEx_PointerEntered(object sender, PointerRoutedEventArgs e)
    => ((StackPanel)((ExpanderEx)sender).FindName("ControlPanel")).Visibility = Visibility.Visible;

    private void ExpanderEx_PointerExited(object sender, PointerRoutedEventArgs e)
        => ((StackPanel)((ExpanderEx)sender).FindName("ControlPanel")).Visibility = Visibility.Collapsed;

    private async void LinkButton_Click(object sender, RoutedEventArgs e)
        => await Launcher.LaunchUriAsync(new Uri(((Dictionary<string, string>)((Button)sender).Tag)["websiteUrl"]));

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => _ = ViewModel.Search();

    private void ClearButton_Click(object sender, RoutedEventArgs e)
        => ViewModel.ModLoaderFilter = ViewModel.VersionFilter = null;

    private void FlyoutButton_Click(object sender, RoutedEventArgs e)
        => DownloaderProcessViewData.CreateModDownloadProcess((CurseForgeModpackViewData)((Button)sender).DataContext, sender as Control);

}

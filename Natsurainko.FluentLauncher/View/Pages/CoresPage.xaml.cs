using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.View.Pages.Property;
using Natsurainko.FluentLauncher.View.Pages.Resources;
using Natsurainko.FluentLauncher.ViewModel.Pages;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Natsurainko.FluentLauncher.View.Pages;

public sealed partial class CoresPage : Page
{
    public CoresPageVM ViewModel { get; set; }

    public CoresPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<CoresPageVM, Page>(this);
        ViewModel.CoresList = CoresList;
    }

    private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        => ((StackPanel)((Border)sender).FindName("ButtonPanel")).Visibility = Visibility.Visible;

    private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        => ((StackPanel)((Border)sender).FindName("ButtonPanel")).Visibility = Visibility.Collapsed;

    private void Property_Click(object sender, RoutedEventArgs e)
        => this.Frame.Navigate(typeof(PropertyPage), ((MenuFlyoutItem)sender).DataContext as GameCoreViewData);

    private void InstallButton_Click(object sender, RoutedEventArgs e)
        => this.Frame.Navigate(typeof(ResourcesInstallPage));

    private void Launch_Click(object sender, RoutedEventArgs e)
    {
        var control = (Control)sender;
        LauncherProcessViewData.CreateLaunchProcess((control.DataContext as GameCoreViewData).Data, sender as Control);
    }

    private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        => await Launcher.LaunchFolderPathAsync((((Control)sender).DataContext as GameCoreViewData).Data.Root.FullName);

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e) => ViewModel.TipsAction.Invoke();
}

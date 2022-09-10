using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Pages.Property;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.View.Pages.Property;

public sealed partial class PropertyDetailsPage : Page
{
    public PropertyDetailsPageVM ViewModel { get; set; }

    public PropertyDetailsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = ViewModelBuilder.Build<PropertyDetailsPageVM, Page>(this);
        ViewModel.Set((GameCoreViewData)e.Parameter);
    }

    private async void Folder_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        => await Launcher.LaunchFolderPathAsync(ViewModel.DisplayGameCore.Data.Root.FullName);
}

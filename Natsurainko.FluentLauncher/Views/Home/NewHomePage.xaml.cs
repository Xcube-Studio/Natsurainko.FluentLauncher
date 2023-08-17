using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Home;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class NewHomePage : Page
{
    public NewHomePage()
    {
        this.InitializeComponent();
        DataContext = App.GetService<HomeViewModel>();
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = DataContext as HomeViewModel;
        splitView.IsPaneOpen = !splitView.IsPaneOpen;

        if (splitView.IsPaneOpen && vm.ActiveGameInfo != null)
            listView.ScrollIntoView(vm.ActiveGameInfo);
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        SharedShadow.Receivers.Add(BackgroundGrid);
        PanelGrid.Translation += new System.Numerics.Vector3(0, 0, 48);
    }
}

using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "CoreManage";

    DefaultViewModel VM => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = (this.DataContext as DefaultViewModel)!;
        vm.InstanceConfig = null!;
    }
}

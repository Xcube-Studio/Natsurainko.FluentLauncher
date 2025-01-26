using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class InstancePage : Page, IBreadcrumbBarAware
{
    public string Route => "Instance";

    InstanceViewModel VM => (InstanceViewModel)DataContext;

    public InstancePage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = (this.DataContext as InstanceViewModel)!;
        vm.InstanceConfig = null!;
    }
}

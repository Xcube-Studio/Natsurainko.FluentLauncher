using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class ConfigPage : Page, IBreadcrumbBarAware
{
    public string Route => "Config";

    public ConfigPage()
    {
        this.InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = (this.DataContext as ConfigViewModel)!;
        vm.inited = false;

        ComboBox.ItemsSource = null; // Unload Binding to AccountService.Accounts
    }
}

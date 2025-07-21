using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Instances;

namespace Natsurainko.FluentLauncher.Views.Instances;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    DefaultViewModel VM => (DefaultViewModel)DataContext;

    string IBreadcrumbBarAware.Route => "Instances";

    public DefaultPage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = null;
        ListView.ItemsSource = null;
    }
}

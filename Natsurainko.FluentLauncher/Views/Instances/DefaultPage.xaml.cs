using FluentLauncher.Infra.UI.Navigation;
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
}

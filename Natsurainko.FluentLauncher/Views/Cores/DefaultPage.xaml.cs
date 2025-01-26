using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    DefaultViewModel VM => (DefaultViewModel)DataContext;

    string IBreadcrumbBarAware.Route => "Cores";

    public DefaultPage()
    {
        InitializeComponent();
    }
}

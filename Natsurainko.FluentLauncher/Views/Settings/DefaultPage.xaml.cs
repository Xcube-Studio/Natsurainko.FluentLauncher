using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "Settings";

    DefaultViewModel VM => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        InitializeComponent();
    }
}

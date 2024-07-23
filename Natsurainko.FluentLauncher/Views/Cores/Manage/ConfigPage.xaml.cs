using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class ConfigPage : Page, IBreadcrumbBarAware
{
    public string Route => "Config";

    public ConfigPage()
    {
        this.InitializeComponent();
    }
}

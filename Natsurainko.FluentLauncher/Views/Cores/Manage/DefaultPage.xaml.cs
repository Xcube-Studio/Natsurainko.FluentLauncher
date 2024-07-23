using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "CoreManage";

    public DefaultPage()
    {
        this.InitializeComponent();
    }
}

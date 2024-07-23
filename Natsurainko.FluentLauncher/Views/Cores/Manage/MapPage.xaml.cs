using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class MapPage : Page, IBreadcrumbBarAware
{
    public string Route => "Map";

    public MapPage()
    {
        this.InitializeComponent();
    }
}

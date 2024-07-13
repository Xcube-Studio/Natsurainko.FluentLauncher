using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "Settings";

    public DefaultPage()
    {
        this.InitializeComponent();
    }
}

using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "Download";

    public DefaultPage()
    {
        this.InitializeComponent();
    }
}

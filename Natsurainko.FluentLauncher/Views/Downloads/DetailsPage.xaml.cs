using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class DetailsPage : Page, IBreadcrumbBarAware
{
    public string Route => "Details";

    public DetailsPage()
    {
        this.InitializeComponent();
    }
}

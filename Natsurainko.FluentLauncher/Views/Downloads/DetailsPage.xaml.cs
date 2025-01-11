using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class DetailsPage : Page, IBreadcrumbBarAware
{
    public string Route => "Details";

    DetailsViewModel VM => (DetailsViewModel)DataContext;

    public DetailsPage()
    {
        InitializeComponent();
    }
}

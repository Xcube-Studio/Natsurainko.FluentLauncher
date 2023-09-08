using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Navigation;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CoreInstallWizardPage : Page
{
    private readonly INavigationService _navigationService = App.GetService<INavigationService>();

    public CoreInstallWizardPage()
    {
        InitializeComponent();
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index.Equals(0)) _navigationService.GoBack();
    }
}

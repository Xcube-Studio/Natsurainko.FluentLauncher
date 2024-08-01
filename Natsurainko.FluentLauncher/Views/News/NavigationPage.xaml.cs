using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.ViewModels.News;

namespace Natsurainko.FluentLauncher.Views.News;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    private NavigationViewModel VM => (NavigationViewModel)DataContext;

    public NavigationPage()
    {
        this.InitializeComponent();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (contentFrame.Content as Page) as IBreadcrumbBarAware;
        VM.Routes.Add(breadcrumbBarAware!.Route);
    }
}

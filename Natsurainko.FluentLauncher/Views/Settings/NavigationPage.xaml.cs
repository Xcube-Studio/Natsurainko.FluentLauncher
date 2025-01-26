using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    INavigationService INavigationProvider.NavigationService => VM.NavigationService;

    private NavigationViewModel VM => (NavigationViewModel)DataContext;

    public NavigationPage()
    {
        InitializeComponent();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (IBreadcrumbBarAware)(contentFrame.Content);
        if (e.NavigationMode == NavigationMode.Back)
            breadcrumbBar.GoBack();
        else
            breadcrumbBar.AddItem(breadcrumbBarAware.Route);
    }

    private void breadcrumbBar_ItemClicked(object sender, string[] args)
    {
        VM.HandleNavigationBreadcrumBarItemClicked(args);
    }
}

using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;

    NavigationViewModel VM => (NavigationViewModel)DataContext;

    public NavigationPage()
    {
        InitializeComponent();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (contentFrame.Content as Page) as IBreadcrumbBarAware;
        VM.Routes.Add(breadcrumbBarAware!.Route);

        if (e.SourcePageType == typeof(DefaultPage))
            VM.Routes.Add(VM.MinecraftInstance.GetDisplayName());
    }
}

using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    INavigationService INavigationProvider.NavigationService => VM.NavigationService;

    NavigationViewModel VM => (NavigationViewModel)DataContext;

    public NavigationPage()
    {
        InitializeComponent();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (IBreadcrumbBarAware)(contentFrame.Content);
        if (e.NavigationMode == NavigationMode.Back)
        {
            breadcrumbBar.GoBack();
        }
        else
        {
            breadcrumbBar.AddItem(breadcrumbBarAware.Route);
            //if (e.SourcePageType == typeof(DefaultPage))
            //    breadcrumbBar.AddItem(VM.MinecraftInstance.GetDisplayName());
        }

    }

    private void breadcrumbBar_ItemClicked(object sender, string[] args)
    {
        VM.HandleNavigationBreadcrumBarItemClicked(args);
    }

    //private void breadcrumbBar_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    //{
    //    VM.HandleBreadcrumbBarLoading(breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"]);
    //}

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        breadcrumbBar.Items = VM.DisplayedPath;
    }
}

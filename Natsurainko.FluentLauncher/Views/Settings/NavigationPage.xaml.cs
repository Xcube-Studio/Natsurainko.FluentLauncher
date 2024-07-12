using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    private NavigationViewModel VM => (NavigationViewModel)DataContext;

    public NavigationPage()
    {
        InitializeComponent();
    }

    #region Sync NavigationViewItem selection

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var navItem = (NavigationViewItem)args.InvokedItemContainer;
        VM.NavigateTo(navItem.GetTag());
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (contentFrame.Content as Page)!.DataContext as IBreadcrumbBarAware;
        VM.Routes.Add(breadcrumbBarAware!.Route);
    }

    #endregion
}

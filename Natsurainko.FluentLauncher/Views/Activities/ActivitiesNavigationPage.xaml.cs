using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Activities;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.Activities;

public sealed partial class ActivitiesNavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    private ActivitiesNavigationViewModel VM => (ActivitiesNavigationViewModel)DataContext;

    public ActivitiesNavigationPage()
    {
        InitializeComponent();
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        => VM.NavigateTo(((NavigationViewItem)args.InvokedItemContainer).GetTag());

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (NavigationViewItem item in NavigationView.MenuItems.Union(NavigationView.FooterMenuItems).Cast<NavigationViewItem>())
        {
            if (App.GetService<IPageProvider>().RegisteredPages[item.GetTag()].PageType == e.SourcePageType)
            {
                NavigationView.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
        }
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Pages;
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
        => VM.NavigateTo(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString());

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (NavigationViewItem item in NavigationView.MenuItems.Union(NavigationView.FooterMenuItems).Cast<NavigationViewItem>())
        {
            if (App.GetService<IPageProvider>().RegisteredPages[item.Tag.ToString()].PageType == e.SourcePageType)
            {
                NavigationView.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
        }
    }
}

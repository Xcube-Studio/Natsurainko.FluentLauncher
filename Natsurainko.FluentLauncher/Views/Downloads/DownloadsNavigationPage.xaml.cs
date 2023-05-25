using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.Downloads;

// TODO: Use NavigationView and add pages for installing new games, mods, and modpacks.

public sealed partial class DownloadsNavigationPage : Page
{
    public DownloadsNavigationPage()
    {
        this.InitializeComponent();
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        => contentFrame.Navigate(Type.GetType(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()));

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (NavigationViewItem item in NavigationView.MenuItems.Union(NavigationView.FooterMenuItems).Cast<NavigationViewItem>())
        {
            if (Type.GetType((string)item.Tag) == e.SourcePageType)
            {
                NavigationView.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
        }
    }
}

using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Activities;

public sealed partial class ActivitiesPage : Page
{
    public ActivitiesPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not null and Type)
            contentFrame.Navigate((Type)e.Parameter);
        else contentFrame.Navigate(typeof(ActivityLaunchPage));
    }

    private void NavigationView_ItemInvoked(Muxc.NavigationView sender, Muxc.NavigationViewItemInvokedEventArgs args)
    {
        if (((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag != null)
            contentFrame.Navigate(Type.GetType($"Natsurainko.FluentLauncher.View.Pages.Activities.{((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag}"));
    }

    private void contentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (Muxc.NavigationViewItem item in this.NavigationView.MenuItems.Union(this.NavigationView.FooterMenuItems))
        {
            if ((string)item.Tag == e.SourcePageType.Name)
            {
                NavigationView.SelectedItem = item;
                item.IsSelected = true;
                return;
            }
        }
    }
}

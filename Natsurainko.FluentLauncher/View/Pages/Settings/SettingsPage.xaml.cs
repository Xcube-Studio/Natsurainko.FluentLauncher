using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Settings;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
        => contentFrame.Navigate(e.Parameter as Type ?? typeof(SettingLaunchPage));

    private void NavigationView_ItemInvoked(Muxc.NavigationView sender, Muxc.NavigationViewItemInvokedEventArgs args)
    {
        if (((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag != null)
            contentFrame.Navigate(Type.GetType($"Natsurainko.FluentLauncher.View.Pages.Settings.{((Muxc.NavigationViewItem)args.InvokedItemContainer).Tag}"));
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (Muxc.NavigationViewItem item in NavigationView.MenuItems.Union(NavigationView.FooterMenuItems).Cast<Muxc.NavigationViewItem>())
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

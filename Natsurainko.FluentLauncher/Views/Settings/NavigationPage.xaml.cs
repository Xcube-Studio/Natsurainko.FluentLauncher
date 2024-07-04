using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Settings;
using System;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    private SettingsNavigationViewModel VM => (SettingsNavigationViewModel)DataContext;

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

    #endregion
}

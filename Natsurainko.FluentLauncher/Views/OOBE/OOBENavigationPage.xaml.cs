using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Pages;
using Natsurainko.FluentLauncher.ViewModels;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using System;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class OOBENavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    private OOBENavigationViewModel VM => (OOBENavigationViewModel)DataContext;

    public OOBENavigationPage()
    {
        InitializeComponent();
        contentFrame.NavigationStopped += ContentFrame_NavigationStopped;
    }

    // Change navigation transition effect after the first navigation
    private void ContentFrame_NavigationStopped(object sender, NavigationEventArgs e)
    {
        contentFrame.Navigated -= ContentFrame_NavigationStopped;
        navTransition.DefaultNavigationTransitionInfo = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
    }

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

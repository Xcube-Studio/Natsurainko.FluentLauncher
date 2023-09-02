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
    private OOBEViewModel VM => (OOBEViewModel)DataContext;

    public OOBENavigationPage()
    {
        InitializeComponent();
        contentFrame.Navigated += ContentFrame_Navigated1;
    }

    // Change navigation transition effect after the first navigation
    int navigationCount = 0;
    private void ContentFrame_Navigated1(object sender, NavigationEventArgs e)
    {
        if (navigationCount == 1)
        {
            navTransition.DefaultNavigationTransitionInfo = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
            contentFrame.Navigated -= ContentFrame_Navigated1;
            return;
        }
        navigationCount++;
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

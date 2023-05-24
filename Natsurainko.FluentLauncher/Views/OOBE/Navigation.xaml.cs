using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class Navigation : Page
{
    public Navigation()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
        => contentFrame.Navigate(typeof(Views.OOBE.Language), null, new DrillInNavigationTransitionInfo());

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

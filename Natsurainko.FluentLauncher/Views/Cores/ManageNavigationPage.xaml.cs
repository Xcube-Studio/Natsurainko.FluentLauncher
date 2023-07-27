using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Cores.Manage;
using System;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class ManageNavigationPage : Page
{
    private ExtendedGameInfo _gameInfo;

    public ManageNavigationPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _gameInfo = e.Parameter as ExtendedGameInfo;

        BreadcrumbBar.ItemsSource = new string[]
        {
            ResourceUtils.GetValue("Cores", "ManageNavigationPage", "_BreadcrumbBar_First"),
            _gameInfo.Name
        };
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index.Equals(0))
            ShellPage.ContentFrame.GoBack();
    }

    private void NavigationView_ItemInvoked(NavigationView _, NavigationViewItemInvokedEventArgs args)
    {
        contentFrame.Navigate(Type.GetType(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()), _gameInfo);
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        contentFrame.Navigate(typeof(CoreSettingsPage), _gameInfo);
    }
}

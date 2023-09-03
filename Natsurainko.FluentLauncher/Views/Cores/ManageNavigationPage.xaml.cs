using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class ManageNavigationPage : Page, INavigationProvider
{
    private ManageNavigationViewModel VM => (ManageNavigationViewModel)DataContext;
    object INavigationProvider.NavigationControl => contentFrame;

    public ManageNavigationPage()
    {
        this.InitializeComponent();
    }

    private void NavigationView_ItemInvoked(NavigationView _, NavigationViewItemInvokedEventArgs args)
        => VM.NavigateTo(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString(), VM._gameInfo);
}

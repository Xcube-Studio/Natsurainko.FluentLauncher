using Natsurainko.FluentLauncher.Class.AppData;
using ReactiveUI.Fody.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages;

public class AccountLoginPageVM : ViewModelBase<Page>
{
    public AccountLoginPageVM(Page control) : base(control)
    {
        Margin = new Thickness(0, GlobalResources.NavigationViewDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal ? 15 : 0, 0, 0);
        GlobalResources.NavigationViewDisplayModeChanged += GlobalResources_NavigationViewDisplayModeChanged;

        control.Unloaded += Control_Unloaded;
    }

    [Reactive]
    public Thickness Margin { get; set; }

    [Reactive]
    public Visibility WebBrowserLoading { get; set; } = Visibility.Collapsed;

    private void Control_Unloaded(object sender, RoutedEventArgs e)
        => Control.Unloaded -= Control_Unloaded;

    private void GlobalResources_NavigationViewDisplayModeChanged(object sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode e)
        => Margin = new Thickness(0, e == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal ? 15 : 0, 0, 0);
}

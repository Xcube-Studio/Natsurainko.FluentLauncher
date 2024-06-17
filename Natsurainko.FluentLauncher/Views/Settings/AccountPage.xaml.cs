using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AccountPage : Page
{
    public AccountPage()
    {
        InitializeComponent();

        this.Loaded += AccountPage_Loaded;
        this.Unloaded += AccountPage_Unloaded;
    }

    private void AccountPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = this.DataContext as AccountViewModel;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(vm!, (r, m) =>
        {
            AccountViewModel vm = (r as AccountViewModel)!;
            vm.ActiveAccount = m.Value;
        });
    }

    private void AccountPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = this.DataContext as AccountViewModel;
        WeakReferenceMessenger.Default.Unregister<ActiveAccountChangedMessage>(vm!);
    }
}

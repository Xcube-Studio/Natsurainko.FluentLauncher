using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Settings;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AccountPage : Page, IBreadcrumbBarAware
{
    public string Route => "Account";

    AccountViewModel VM => (AccountViewModel)DataContext;

    public AccountPage()
    {
        InitializeComponent();

        Loaded += AccountPage_Loaded;
        Unloaded += AccountPage_Unloaded;
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

    bool IsLoadLastRefreshCard(AccountType accountType) => accountType == AccountType.Microsoft;
}

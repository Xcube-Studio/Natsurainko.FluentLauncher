using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AccountPage : Page, IBreadcrumbBarAware
{
    public string Route => "Account";

    AccountViewModel VM => (AccountViewModel)DataContext;

    public AccountPage()
    {
        InitializeComponent();
    }
}

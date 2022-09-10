using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.ViewModel.Pages.Settings;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Settings;

public sealed partial class SettingAccountPage : Page
{
    public SettingAccountPageVM ViewModel { get; set; }

    public SettingAccountPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<SettingAccountPageVM, Page>(this);
    }

    private void RemoveAccount_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Accounts.Remove(ViewModel.CurrentAccount);
        ViewModel.CurrentAccount = ViewModel.Accounts.Any() ? ViewModel.Accounts[0] : null;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
        => MainContainer.ContentFrame.Navigate(typeof(AccountLoginPage));
}

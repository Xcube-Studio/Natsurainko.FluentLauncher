using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AccountPage : Page
{
    public bool RemoveBool = false;
    public Visibility RemoveVisible => (DataContext as AccountViewModel).ActiveAccount is null ? Visibility.Collapsed : Visibility.Visible;

    public AccountPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<AccountViewModel>();
    }

    public Visibility a() => Visibility.Visible;
}

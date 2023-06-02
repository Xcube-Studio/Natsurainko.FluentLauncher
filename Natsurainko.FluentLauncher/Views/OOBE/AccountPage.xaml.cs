using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class AccountPage : Page
{
    public AccountPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<AccountViewModel>();
    }
}

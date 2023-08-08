using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Home;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = App.GetService<HomeViewModel>();
    }
}

using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Home;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class NewHomePage : Page
{
    public NewHomePage()
    {
        this.InitializeComponent();
        DataContext = App.GetService<HomeViewModel>();
    }
}

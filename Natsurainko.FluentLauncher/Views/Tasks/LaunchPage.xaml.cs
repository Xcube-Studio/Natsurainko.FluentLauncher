using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Tasks;

namespace Natsurainko.FluentLauncher.Views.Tasks;

public sealed partial class LaunchPage : Page
{
    LaunchViewModel VM => (LaunchViewModel)DataContext;

    public LaunchPage()
    {
        InitializeComponent();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class LaunchPage : Page
{
    public LaunchPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<LaunchViewModel>();
    }
}

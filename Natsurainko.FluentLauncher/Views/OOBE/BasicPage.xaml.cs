using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class BasicPage : Page
{
    public BasicPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<BasicViewModel>();
    }
}

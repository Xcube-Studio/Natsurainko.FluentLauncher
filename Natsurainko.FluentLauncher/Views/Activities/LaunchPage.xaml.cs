using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Activities;

namespace Natsurainko.FluentLauncher.Views.Activities;

public sealed partial class LaunchPage : Page
{
    public LaunchPage()
    {
        InitializeComponent();
        this.DataContext = App.GetService<LaunchViewModel>();
    }
}

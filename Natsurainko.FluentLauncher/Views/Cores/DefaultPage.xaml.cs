using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class DefaultPage : Page
{
    DefaultViewModel VM => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        InitializeComponent();
    }
}

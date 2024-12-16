using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Install;

namespace Natsurainko.FluentLauncher.Views.Cores.Install;

public sealed partial class ConfigPage : Page
{
    ConfigViewModel VM => (ConfigViewModel)DataContext;

    public ConfigPage()
    {
        InitializeComponent();
    }
}

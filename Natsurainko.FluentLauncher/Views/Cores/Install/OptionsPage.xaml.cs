using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Install;

namespace Natsurainko.FluentLauncher.Views.Cores.Install;

public sealed partial class OptionsPage : Page
{
    OptionsViewModel VM => (OptionsViewModel)DataContext;

    public OptionsPage()
    {
        InitializeComponent();
    }
}

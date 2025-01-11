using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Install;

namespace Natsurainko.FluentLauncher.Views.Cores.Install;

public sealed partial class ChoosePage : Page
{
    ChooseViewModel VM => (ChooseViewModel)DataContext;

    public ChoosePage()
    {
        InitializeComponent();
    }
}

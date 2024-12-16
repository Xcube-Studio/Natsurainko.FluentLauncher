using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class CoresPage : Page
{
    CoresViewModel VM => (CoresViewModel)DataContext;

    public CoresPage()
    {
        InitializeComponent();
    }
}

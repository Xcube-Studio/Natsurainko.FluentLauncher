using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class CoresPage : Page
{
    public CoresPage()
    {
        this.InitializeComponent();
        DataContext = App.GetService<CoresViewModel>();
    }
}

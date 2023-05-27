using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Components.FluentCore;

namespace Natsurainko.FluentLauncher.Views.Cores.Properties;

public sealed partial class LaunchPage : Page
{
    public LaunchPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Cores.Properties.LaunchViewModel(e.Parameter as GameCore);
    }
}

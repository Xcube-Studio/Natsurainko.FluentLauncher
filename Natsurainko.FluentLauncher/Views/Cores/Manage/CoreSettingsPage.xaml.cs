using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class CoreSettingsPage : Page
{
    public CoreSettingsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new CoreSettingsViewModel(e.Parameter as ExtendedGameInfo);
    }
}

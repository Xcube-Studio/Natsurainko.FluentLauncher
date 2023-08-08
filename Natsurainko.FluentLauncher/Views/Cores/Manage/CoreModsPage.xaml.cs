using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;
using Nrk.FluentCore.Classes.Datas;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class CoreModsPage : Page
{
    public CoreModsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new CoreModsViewModel(e.Parameter as ExtendedGameInfo);
    }

    private void toggleSwitch_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        void ToggleSwitch_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var modInfo = toggleSwitch.DataContext as ModInfo;
            var modsManager = (this.DataContext as CoreModsViewModel).modsManager;

            if (modInfo != null)
                modsManager.Switch(modInfo, toggleSwitch.IsOn);
        }

        var toggleSwitch = sender as ToggleSwitch;

        toggleSwitch.Toggled += ToggleSwitch_Toggled;
        toggleSwitch.Unloaded += (_, _) => toggleSwitch.Toggled -= ToggleSwitch_Toggled;
    }
}

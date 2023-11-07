using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;
using Nrk.FluentCore.Management;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class CoreModsPage : Page
{
    public CoreModsPage()
    {
        this.InitializeComponent();
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

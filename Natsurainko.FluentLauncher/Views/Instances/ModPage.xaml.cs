using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Instances;
using Nrk.FluentCore.GameManagement.Mods;

namespace Natsurainko.FluentLauncher.Views.Instances;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    public string Route => "Mod";

    ModViewModel VM => (ModViewModel)DataContext;

    public ModPage()
    {
        InitializeComponent();
    }

    private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
    {
        void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            var modInfo = (MinecraftMod)toggleSwitch.DataContext;
            var modsManager = ((ModViewModel)DataContext).ModsManager;

            if (modInfo != null)
                modsManager.Switch(modInfo, toggleSwitch.IsOn);
        }

        var toggleSwitch = (ToggleSwitch)sender;

        toggleSwitch.Toggled += ToggleSwitch_Toggled;
        toggleSwitch.Unloaded += (_, _) => toggleSwitch.Toggled -= ToggleSwitch_Toggled;
    }
}

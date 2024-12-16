using CommunityToolkit.WinUI.Controls;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores.Manage;
using Nrk.FluentCore.GameManagement.Mods;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    public string Route => "Mod";

    ModViewModel VM => (ModViewModel)DataContext;

    public ModPage()
    {
        InitializeComponent();
    }

    private void SettingsCard_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var card = (sender as SettingsCard)!;
        var textBlock = (card.FindName("InfoText") as TextBlock)!;

        textBlock.TextWrapping = Microsoft.UI.Xaml.TextWrapping.WrapWholeWords;
        textBlock.TextTrimming = Microsoft.UI.Xaml.TextTrimming.None;
        textBlock.MaxLines = 0;
    }

    private void SettingsCard_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var card = (sender as SettingsCard)!;
        var textBlock = (card.FindName("InfoText") as TextBlock)!;

        textBlock.TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap;
        textBlock.TextTrimming = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis;
        textBlock.MaxLines = 1;
    }

    private void ToggleSwitch_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        void ToggleSwitch_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

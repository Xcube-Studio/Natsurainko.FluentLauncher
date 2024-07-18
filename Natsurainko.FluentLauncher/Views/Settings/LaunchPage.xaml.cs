using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class LaunchPage : Page, IBreadcrumbBarAware
{
    public string Route => "Launch";

    public LaunchPage()
    {
        InitializeComponent();
    }

    private void Card_Loaded(object sender, RoutedEventArgs e)
    {
        var card = (sender as SettingsCard)!;
        var path = card.Description.ToString();
        var tag = card.Tag.ToString();

        var current = tag == "ActiveMinecraftFolder"
            ? App.GetService<SettingsService>().ActiveMinecraftFolder
            : App.GetService<SettingsService>().ActiveJava;

        var defaultBackground = card.Background;
        var defaultTheme = card.ActualTheme;

        WeakReferenceMessenger.Default.Register<SettingsStringValueChangedMessage>(sender, (r, m) =>
        {
            if (m.PropertyName != tag)
                return;

            card.Background = path == m.Value ? (Brush)this.Resources["AccentButtonBackground"] : defaultBackground;
            card.RequestedTheme = path == m.Value ? ElementTheme.Light : defaultTheme;
        });

        card.Background = path == current ? (Brush)this.Resources["AccentButtonBackground"] : defaultBackground;
        card.RequestedTheme = path == current ? ElementTheme.Light : defaultTheme;

        card.Unloaded += (s, e) => WeakReferenceMessenger.Default.Unregister<SettingsStringValueChangedMessage>(sender);
    }
}

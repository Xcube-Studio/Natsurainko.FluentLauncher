using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Settings;
using System.IO;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class LaunchPage : Page, IBreadcrumbBarAware
{
    public string Route => "Launch";

    LaunchViewModel VM => (LaunchViewModel)DataContext;

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

            var enableText = card.FindName("EnableText") as Border;
            enableText!.Visibility = path == m.Value ? Visibility.Visible : Visibility.Collapsed;
        });

        var enableText = card.FindName("EnableText") as Border;
        enableText!.Visibility = path == current ? Visibility.Visible : Visibility.Collapsed;

        card.Unloaded += (s, e) => WeakReferenceMessenger.Default.Unregister<SettingsStringValueChangedMessage>(sender);
    }

    internal static bool CanJavaActivate(string java)
    {
        try
        {
            return File.Exists(java);
        }
        catch { }

        return false;
    }
}

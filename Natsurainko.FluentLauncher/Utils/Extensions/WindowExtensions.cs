using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using System.IO;
using Windows.ApplicationModel;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class WindowExtensions
{
    public static void ConfigureTitleBarTheme(this WindowEx window)
    {
        FrameworkElement frameworkElement = ((FrameworkElement)window.Content);
        ElementTheme actualTheme = frameworkElement.ActualTheme;
        AppWindow appWindow = window.AppWindow;

        var hoverColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
        appWindow.TitleBar.ButtonBackgroundColor = appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        appWindow.TitleBar.ButtonForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        appWindow.TitleBar.ButtonHoverForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        appWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        frameworkElement.ActualThemeChanged += (frameworkElement, _) =>
        {
            ElementTheme actualTheme = frameworkElement.ActualTheme;

            appWindow.TitleBar.ButtonBackgroundColor = appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            appWindow.TitleBar.ButtonHoverForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;

            var hoverColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            hoverColor.A = 35;

            appWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
        };
    }

    public static void ConfigureElementTheme(this Window window)
    {
        if (window.Content is not FrameworkElement frameworkElement)
            return;

        ElementTheme theme = (ElementTheme)App.GetService<SettingsService>().DisplayTheme;
        frameworkElement.RequestedTheme = theme;

        WeakReferenceMessenger.Default.Register<SettingsRequestThemeChangedMessage>(window, (sender, m) => frameworkElement.RequestedTheme = m.Value);
    }
}

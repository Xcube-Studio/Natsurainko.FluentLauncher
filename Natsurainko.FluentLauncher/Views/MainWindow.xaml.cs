using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using System.IO;
using Windows.ApplicationModel;
using Windows.Globalization;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class MainWindow : WindowEx
{
    public Frame ContentFrame => Frame;

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly NotificationService _notificationService = App.GetService<NotificationService>();

    public MainWindow()
    {
        if (string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
            ResourceUtils.ApplyLanguage(_settings.CurrentLanguage);

        InitializeComponent();

        _notificationService.InitContainer(NotifyStackPanel, BackgroundGrid);

        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));

        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;

        var hoverColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;

        (MinWidth, MinHeight) = (516, 328);
        (Width, Height) = (_settings.AppWindowWidth, _settings.AppWindowHeight);

        Frame.Navigate(_settings.FinishGuide ? typeof(ShellPage) : typeof(OOBE.OOBENavigationPage));
    }
}

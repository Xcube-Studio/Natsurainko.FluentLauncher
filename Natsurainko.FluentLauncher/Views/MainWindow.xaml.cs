using FluentLauncher.Infra.UI.Navigation;
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

public sealed partial class MainWindow : WindowEx, INavigationProvider
{
    public Frame ContentFrame => Frame;

    object INavigationProvider.NavigationControl => Frame;

    private readonly INavigationService _navService;
    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly NotificationService _notificationService = App.GetService<NotificationService>();
    private bool _firstActivated = true;

    public MainWindow(INavigationService navService)
    {
        _navService = navService;
        App.MainWindow = this;
        var hoverColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

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
        AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;

        (MinWidth, MinHeight) = _settings.FinishGuide ? (516, 328) : (_settings.AppWindowWidth, _settings.AppWindowHeight);
        (Width, Height) = (_settings.AppWindowWidth, _settings.AppWindowHeight);

        App.GetService<AppearanceService>().ApplyBackgroundAtWindowCreated(this);

        ((FrameworkElement)this.Content).ActualThemeChanged += MainWindow_ActualThemeChanged;
        this.WindowStateChanged += MainWindow_WindowStateChanged;
        this.SizeChanged += MainWindow_SizeChanged;
        this.Activated += MainWindow_Activated;
    }

    #region Window Event

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_firstActivated)
        {
            _navService.NavigateTo(_settings.FinishGuide ? "ShellPage" : "OOBENavigationPage");
            this.CenterOnScreen();
            if (_settings.AppWindowState == WindowState.Maximized) this.Maximize();
        }

        _firstActivated = false;
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        _settings.AppWindowWidth = App.MainWindow.Width;
        _settings.AppWindowHeight = App.MainWindow.Height;
    }

    private void MainWindow_WindowStateChanged(object? sender, WindowState e)
    {
        _settings.AppWindowState = e;
    }

    private void MainWindow_ActualThemeChanged(FrameworkElement sender, object args)
    {
        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;

        var hoverColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
    }
    #endregion

    public void NavigateToLaunchTasksPage() => _navService.NavigateTo("ShellPage", "ActivitiesNavigationPage");
}

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

    private readonly INavigationService _navigationService;
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;

    public MainWindow(
        SettingsService settingsService,
        NotificationService notificationService, 
        INavigationService navigationService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;
        _navigationService = navigationService;

        App.MainWindow = this;

        if (string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
            ResourceUtils.ApplyLanguage(_settingsService.CurrentLanguage);

        InitializeComponent();
        ConfigureWindow();
    }

    #region Window Event

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        _navigationService.NavigateTo(_settingsService.FinishGuide ? "ShellPage" : "OOBENavigationPage");

        this.CenterOnScreen();

        if (_settingsService.AppWindowState == WindowState.Maximized)
            this.Maximize();

        this.Activated -= MainWindow_Activated;
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        _settingsService.AppWindowWidth = App.MainWindow.Width;
        _settingsService.AppWindowHeight = App.MainWindow.Height;
    }

    private void MainWindow_WindowStateChanged(object? sender, WindowState e)
    {
        _settingsService.AppWindowState = e;
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

    void ConfigureWindow()
    {
        var hoverColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        _notificationService.InitContainer(NotifyStackPanel, BackgroundGrid);

        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;

        (MinWidth, MinHeight) = _settingsService.FinishGuide ? (516, 328) : (_settingsService.AppWindowWidth, _settingsService.AppWindowHeight);
        (Width, Height) = (_settingsService.AppWindowWidth, _settingsService.AppWindowHeight);

        App.GetService<AppearanceService>().ApplyBackgroundAtWindowCreated(this);

        ((FrameworkElement)this.Content).ActualThemeChanged += MainWindow_ActualThemeChanged;
        this.WindowStateChanged += MainWindow_WindowStateChanged;
        this.SizeChanged += MainWindow_SizeChanged;
        this.Activated += MainWindow_Activated;
    }

    public void NavigateToLaunchTasksPage() => _navigationService.NavigateTo("ShellPage", "ActivitiesNavigationPage");
}

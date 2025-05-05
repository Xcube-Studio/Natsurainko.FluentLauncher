using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using System.IO;
using Windows.ApplicationModel;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class MainWindow : WindowEx, INavigationProvider
{
    public Frame ContentFrame => Frame;

    public static XamlRoot XamlRoot { get; set; } = null!;

    private readonly INavigationService _navigationService;
    private readonly SettingsService _settingsService;

    object INavigationProvider.NavigationControl => Frame;
    INavigationService INavigationProvider.NavigationService => _navigationService;

    public MainWindow(
        SettingsService settingsService,
        INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;

        App.MainWindow = this;
        App.GetService<AppearanceService>().RegisterWindow(this);
        
        InitializeComponent();
        ConfigureWindow();
    }

    #region Window Event

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
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
        var titleBarTheme = BackgroundGrid.ActualTheme;

        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = titleBarTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverForegroundColor = titleBarTheme == ElementTheme.Light ? Colors.Black : Colors.White;

        var hoverColor = titleBarTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
    }

    #endregion

    void ConfigureWindow()
    {
        var titleBarTheme = _settingsService.UseBackgroundMask ? ApplicationTheme.Light : App.Current.RequestedTheme;
        var hoverColor = BackgroundGrid.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        App.GetService<InfoBarPresenter>().InitializeContainer(StackPanel);
        App.GetService<Services.UI.NotificationService>().InitContainer(NotifyStackPanel, BackgroundGrid);

        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = BackgroundGrid.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverForegroundColor = BackgroundGrid.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        (MinWidth, MinHeight) = _settingsService.FinishGuide ? (516, 328) : (_settingsService.AppWindowWidth, _settingsService.AppWindowHeight);
        (Width, Height) = (_settingsService.AppWindowWidth, _settingsService.AppWindowHeight);

        ((FrameworkElement)this.Content).ActualThemeChanged += MainWindow_ActualThemeChanged;
        this.WindowStateChanged += MainWindow_WindowStateChanged;
        this.SizeChanged += MainWindow_SizeChanged;
        this.Activated += MainWindow_Activated;
    }

    private void Frame_Loaded(object sender, RoutedEventArgs e)
    {
        XamlRoot = Frame.XamlRoot;
        _navigationService.NavigateTo(_settingsService.FinishGuide ? "ShellPage" : "OOBENavigationPage");

        _settingsService.CurrentLanguageChanged += (_, _) =>
            _navigationService.NavigateTo(_settingsService.FinishGuide ? "ShellPage" : "OOBENavigationPage");
    }
}

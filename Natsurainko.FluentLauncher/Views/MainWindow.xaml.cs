using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Windows.UI.WindowManagement;
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

    public MainWindow(SettingsService settingsService, INavigationService navigationService)
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

        NotificationsScrollViewer.Margin = Grid.ActualWidth >= 641
            ? new Thickness(48, 48, 0, 0)
            : new Thickness(0, 48, 0, 0);
    }

    private void MainWindow_WindowStateChanged(object? sender, WindowState e)   
    {
        _settingsService.AppWindowState = e;
    }

    #endregion

    void ConfigureWindow()
    {
        this.ConfigureTitleBarTheme();
        App.GetService<InfoBarPresenter>().InitializeContainer(StackPanel);

        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        (MinWidth, MinHeight) = _settingsService.FinishGuide ? (516, 328) : (_settingsService.AppWindowWidth, _settingsService.AppWindowHeight);
        (Width, Height) = (_settingsService.AppWindowWidth, _settingsService.AppWindowHeight);

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

        NotificationsScrollViewer.Margin = Grid.ActualWidth >= 641
            ? new Thickness(48, 48, 0, 0)
            : new Thickness(0, 48, 0, 0);
    }
}

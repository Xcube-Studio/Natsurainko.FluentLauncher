using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using Windows.Win32;
using WinRT.Interop;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class MainWindow : WindowEx, INavigationProvider, IRecipient<LanguageChangedMessage>
{
    public Frame ContentFrame => Frame;

    public static XamlRoot XamlRoot { get; set; } = null!;

    private readonly INavigationService _navigationService;
    private readonly ILogger<MainWindow> _logger;
    private readonly SettingsService _settingsService;

    object INavigationProvider.NavigationControl => Frame;
    INavigationService INavigationProvider.NavigationService => _navigationService;

    public MainWindow(
        SettingsService settingsService,
        INavigationService navigationService, 
        ILogger<MainWindow> logger)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _logger = logger;

        App.MainWindow = this;
        App.GetService<AppearanceService>().RegisterWindow(this);

        WeakReferenceMessenger.Default.Register(this);

        InitializeComponent();
        ConfigureWindow();
    }

    #region Window Events

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        NotificationsScrollViewer.Margin = new Thickness(Grid.ActualWidth >= 641 ? 48 : 0, 48, 0, 0);
        _settingsService.WINDOWPLACEMENT = this.GetCurrentWindowPlacement();
    }

    private void MainWindow_PositionChanged(object? sender, PointInt32 e)
    {
        _settingsService.WINDOWPLACEMENT = this.GetCurrentWindowPlacement();
    }

    #endregion

    void ConfigureWindow()
    {
        this.ConfigureTitleBarTheme();
        this.ConfigureElementTheme();

        App.GetService<InfoBarPresenter>().InitializeContainer(StackPanel);

        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        (MinWidth, MinHeight) = _settingsService.FinishGuide ? (516, 328) : (1235, 675);

        if (_settingsService.WINDOWPLACEMENT.length == 0)
        {
            (Width, Height) = (1235, 675);
            this.CenterOnScreen();
        }
        else
        {
            PInvoke.SetWindowPlacement(
                new Windows.Win32.Foundation.HWND(WindowNative.GetWindowHandle(this)),
                _settingsService.WINDOWPLACEMENT);
        }

        this.BringToFront();

        this.SizeChanged += MainWindow_SizeChanged;
        this.PositionChanged += MainWindow_PositionChanged;

        _logger.MainWindowInitialized();
    }

    private void Frame_Loaded(object sender, RoutedEventArgs e)
    {
        XamlRoot = Frame.XamlRoot;
        _navigationService.NavigateTo(_settingsService.FinishGuide ? "ShellPage" : "OOBENavigationPage");

        NotificationsScrollViewer.Margin = new Thickness(Grid.ActualWidth >= 641 ? 48 : 0, 48, 0, 0);
    }

    void IRecipient<LanguageChangedMessage>.Receive(LanguageChangedMessage message)
    {
        if (!_settingsService.FinishGuide)
        {
            _navigationService.NavigateTo("OOBENavigationPage");
            _logger.NavigateToOOBEPage();
        }
        else _navigationService.NavigateTo("ShellPage", "Settings/Navigation");
    }
}

internal static partial class MainWindowLoggers
{
    [LoggerMessage(Level = LogLevel.Information, Message = "MainWindow initialized.")]
    public static partial void MainWindowInitialized(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Launcher OOBE is not finished, navigate to OOBE Page")]
    public static partial void NavigateToOOBEPage(this ILogger logger);
}

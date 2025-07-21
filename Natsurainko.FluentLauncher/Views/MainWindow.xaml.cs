using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using Windows.Win32;
using WinRT.Interop;
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

        if (_settingsService.FinishGuide)
        { 
            PInvoke.SetWindowPlacement(
                new Windows.Win32.Foundation.HWND(WindowNative.GetWindowHandle(this)),
                _settingsService.WINDOWPLACEMENT);
        }
        else
        {
            (Width, Height) = (1235, 675);
            this.CenterOnScreen();
        }

        this.BringToFront();

        this.SizeChanged += MainWindow_SizeChanged;
        this.PositionChanged += MainWindow_PositionChanged;
    }

    private void Frame_Loaded(object sender, RoutedEventArgs e)
    {
        XamlRoot = Frame.XamlRoot;
        _navigationService.NavigateTo(_settingsService.FinishGuide ? "ShellPage" : "OOBENavigationPage");

        _settingsService.CurrentLanguageChanged += (_, _) =>
            _navigationService.NavigateTo(_settingsService.FinishGuide ? "ShellPage" : "OOBENavigationPage");

        NotificationsScrollViewer.Margin = new Thickness(Grid.ActualWidth >= 641 ? 48 : 0, 48, 0, 0);
    }
}

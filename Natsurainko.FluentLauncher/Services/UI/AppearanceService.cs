using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Home;
using System;
using System.IO;
using WinRT;

namespace Natsurainko.FluentLauncher.Services.UI;

internal class AppearanceService
{
    private readonly SettingsService _settingsService;
    private NavigationView _navigationView;
    private BitmapImage backgroundImage;

    public Type HomePageType => _settingsService.UseNewHomePage ? typeof(NewHomePage) : typeof(HomePage);

    public AppearanceService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private void NavigationViewDisplayModeChanged(AppSettingsManagement.SettingsContainer sender, AppSettingsManagement.SettingChangedEventArgs e)
    {
        _navigationView.PaneDisplayMode = _settingsService.NavigationViewDisplayMode == 0
            ? NavigationViewPaneDisplayMode.Auto
            : NavigationViewPaneDisplayMode.LeftMinimal;
    }

    public void RegisterNavigationView(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.PaneDisplayMode = _settingsService.NavigationViewDisplayMode == 0
            ? NavigationViewPaneDisplayMode.Auto
            : NavigationViewPaneDisplayMode.LeftMinimal;

        _settingsService.NavigationViewDisplayModeChanged += NavigationViewDisplayModeChanged;
    }

    public void ApplyDisplayTheme()
    {
        if (_settingsService.DisplayTheme == 0)
            return;

        App.Current.RequestedTheme = _settingsService.DisplayTheme == 1
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark;
    }

    public void ApplyBackgroundBeforePageInit(ShellPage page)
    {
        switch (_settingsService.BackgroundMode)
        {
            case 2:
                if (_settingsService.SolidSelectedIndex == 0)
                    page.Background = App.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush;
                else if (_settingsService.SolidCustomColor != null)
                    page.Background = new SolidColorBrush(_settingsService.SolidCustomColor.GetValueOrDefault(Colors.Transparent));

                break;
            case 3:
                if (File.Exists(_settingsService.ImageFilePath))
                {
                    page.Resources.Add("NavigationViewContentBackground", new SolidColorBrush(Colors.Transparent));
                    page.Resources.Add("NavigationViewContentGridCornerRadius", new CornerRadius(0));
                    page.Resources.Add("NavigationViewContentGridBorderThickness", new Thickness(0));

                    using var fileStream = File.OpenRead(_settingsService.ImageFilePath);
                    using var randomAccessStream = fileStream.AsRandomAccessStream();

                    BitmapImage bitmapImage = new();
                    bitmapImage.SetSource(randomAccessStream);

                    backgroundImage = bitmapImage;
                }
                break;
        }
    }

    public void ApplyBackgroundAfterPageInit(ShellPage page)
    {
        switch (_settingsService.BackgroundMode)
        {
            case 3:
                page.BackgroundImage.Source = backgroundImage;
                int blurred = 0;

                ShellPage.ContentFrame.Navigated += (object sender, NavigationEventArgs e) =>
                {
                    if (!HomePageType.Equals(e.SourcePageType))
                    {
                        if (blurred.Equals(75))
                            return;

                        page.BlurAnimation(blurred, 75);
                        blurred = 75;
                    }
                    else
                    {
                        if (blurred.Equals(0))
                            return;

                        page.BlurAnimation(blurred, 0);
                        blurred = 0;
                    }
                };
                break;
        }
    }

    public void ApplyBackgroundAtWindowCreated(MainWindow window)
    {
        WindowsSystemDispatcherQueueHelper m_wsdqHelper = null;
        DesktopAcrylicController m_backdropController = null;
        SystemBackdropConfiguration m_configurationSource = null;

        bool TrySetAcrylicBackdrop(MainWindow window)
        {
            if (DesktopAcrylicController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                m_configurationSource = new SystemBackdropConfiguration();
                window.Activated += Window_Activated;
                window.Closed += Window_Closed;
                ((FrameworkElement)window.Content).ActualThemeChanged += Window_ThemeChanged;

                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_backdropController = new DesktopAcrylicController();

                if (!_settingsService.EnableDefaultAcrylicBrush)
                {
                    m_backdropController.TintOpacity = (float)_settingsService.TintOpacity;
                    m_backdropController.LuminosityOpacity = (float)_settingsService.TintLuminosityOpacity;
                }

                m_backdropController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }

            return false;
        }

        void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        void Window_Closed(object sender, WindowEventArgs args)
        {
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
            }
            window.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
                SetConfigurationSourceTheme();
        }

        void SetConfigurationSourceTheme()
        {
            m_configurationSource.Theme = ((FrameworkElement)window.Content).ActualTheme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                _ => SystemBackdropTheme.Default
            };
        }

        switch (_settingsService.BackgroundMode)
        {
            case 0:
                if (MicaController.IsSupported())
                    window.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
                break;
            case 1:
                if (DesktopAcrylicController.IsSupported())
                    TrySetAcrylicBackdrop(window);
                break;
        }
    }
}
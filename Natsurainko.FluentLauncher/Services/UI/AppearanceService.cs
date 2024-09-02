using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Home;
using System.IO;

namespace Natsurainko.FluentLauncher.Services.UI;

internal class AppearanceService
{
    private readonly SettingsService _settingsService;
    private NavigationView? _navigationView;
    private BitmapImage? backgroundImage;

    public AppearanceService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void RegisterNavigationView(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.PaneDisplayMode = _settingsService.NavigationViewDisplayMode == 0
            ? NavigationViewPaneDisplayMode.Auto
            : NavigationViewPaneDisplayMode.LeftMinimal;

        _settingsService.NavigationViewDisplayModeChanged += (sender, e) =>
        {
            _navigationView.PaneDisplayMode = _settingsService.NavigationViewDisplayMode == 0
                ? NavigationViewPaneDisplayMode.Auto
                : NavigationViewPaneDisplayMode.LeftMinimal;
        };

        _navigationView.IsPaneOpen = _settingsService.NavigationViewIsPaneOpen;
        _navigationView.PaneOpening += (sender, e) => _settingsService.NavigationViewIsPaneOpen = sender.IsPaneOpen;
        _navigationView.PaneClosing += (sender, e) => _settingsService.NavigationViewIsPaneOpen = sender.IsPaneOpen;
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
                else if (_settingsService.CustomBackgroundColor != null)
                    page.Background = new SolidColorBrush(_settingsService.CustomBackgroundColor.GetValueOrDefault(Colors.Transparent));

                break;
            case 3:
                if (File.Exists(_settingsService.ImageFilePath))
                {
                    page.Resources.Add("NavigationViewContentBackground", new SolidColorBrush(Colors.Transparent));
                    //page.Resources.Add("NavigationViewContentGridCornerRadius", new CornerRadius(0));
                    page.Resources.Add("NavigationViewContentGridBorderThickness", new Thickness(0));
                    page.Resources["BackgroundBorder"] = new Thickness(0);

                    using var fileStream = File.OpenRead(_settingsService.ImageFilePath);
                    using var randomAccessStream = fileStream.AsRandomAccessStream();

                    BitmapImage bitmapImage = new();
                    bitmapImage.SetSource(randomAccessStream);

                    backgroundImage = bitmapImage;
                }
                break;
        }
    }

    public void ApplyThemeColorBeforePageInit()
    {
        App.Current.Resources["RawSystemAccentColor"] = App.Current.Resources["SystemAccentColor"];

        // 窄边框模式
        //App.Current.Resources["PageMarginWithBorder"] = new Thickness(20, 0, 20, 20);
        //App.Current.Resources["PageMarginWithBreadcrumbBar"] = new Thickness(10, 0, 10, 10);
        //App.Current.Resources["PageMarginWithStackPanel"] = new Thickness(20, 0, 20, 20);
        //App.Current.Resources["PagePaddingWithScrollViewer"] = new Thickness(20, 0, 20, 0);
        //App.Current.Resources["PageEndMarginWithScrollViewer"] = new Thickness(0, 0, 0, 20);

        //App.Current.Resources["PagePaddingWithInstallWizardPageGrid"] = new Thickness(20);
        //App.Current.Resources["PagePaddingWithInstallWizardPageStackPanel"] = new Thickness(10);
        //App.Current.Resources["PagePaddingWithInstallWizardPageFrame"] = new Thickness(20,0,20,0);
        //App.Current.Resources["PageMarginWithInstallWizardPageStackPanel"] = new Thickness(10, 0, 10, 0);

        if (!_settingsService.UseSystemAccentColor)
        {
            App.Current.Resources["SystemAccentColorLight1"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            App.Current.Resources["SystemAccentColorLight2"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            App.Current.Resources["SystemAccentColorLight3"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            App.Current.Resources["SystemAccentColorDark1"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            App.Current.Resources["SystemAccentColorDark2"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            App.Current.Resources["SystemAccentColorDark3"] = _settingsService.CustomThemeColor.GetValueOrDefault();

            App.Current.Resources["SystemAccentColor"] = _settingsService.CustomThemeColor.GetValueOrDefault();
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
                    if (!typeof(HomePage).Equals(e.SourcePageType))
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
        switch (_settingsService.BackgroundMode)
        {
            case 0:
                if (MicaController.IsSupported())
                    window.SystemBackdrop = new MicaBackdrop() { Kind = (MicaKind)_settingsService.MicaKind };
                break;
            case 1:
                if (DesktopAcrylicController.IsSupported())
                    window.SystemBackdrop = new DesktopAcrylicBackdrop();
                break;
        }
    }
}
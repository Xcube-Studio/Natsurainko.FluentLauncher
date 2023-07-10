using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Views.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI;

internal class AppearanceService
{
    private readonly SettingsService _settingsService;

    private NavigationView _navigationView;

    public Type HomePageType => _settingsService.UseNewHomePage ? typeof(NewHomePage) : typeof(HomePage);

    public AppearanceService(SettingsService settingsService) 
    {
        _settingsService = settingsService;
    }

    private void DisplayModeChanged(AppSettingsManagement.SettingsContainer sender, AppSettingsManagement.SettingChangedEventArgs e)
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

        _settingsService.NavigationViewDisplayModeChanged += DisplayModeChanged;
    }

    public void ApplyDisplayTheme()
    {
        if (_settingsService.DisplayTheme == 0)
            return;

        App.Current.RequestedTheme = _settingsService.DisplayTheme == 1
            ? Microsoft.UI.Xaml.ApplicationTheme.Light
            : Microsoft.UI.Xaml.ApplicationTheme.Dark;
    }
}

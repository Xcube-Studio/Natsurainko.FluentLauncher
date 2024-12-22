using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;

namespace Natsurainko.FluentLauncher.Services.UI;

internal class AppearanceService
{
    private readonly SettingsService _settingsService;

    public event EventHandler? BackgroundReloaded;

    public AppearanceService(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _settingsService.DisplayThemeChanged += DisplayThemeChanged;
        _settingsService.UseSystemAccentColorChanged += UseSystemAccentColorChanged;
        _settingsService.BackgroundModeChanged += BackgroundModeChanged;

        _settingsService.MicaKindChanged += PropertyChanged;
        _settingsService.SolidSelectedIndexChanged += PropertyChanged;
        _settingsService.CustomBackgroundColorChanged += PropertyChanged;
        _settingsService.ImageFilePathChanged += PropertyChanged;
    }

    private bool IsAppRegistered;

    private bool IsWindowRegistered;

    public App? RegisteredApp { get; private set; }

    public Window? RegisteredWindow { get; private set; }

    public void RegisterWindow(Window window)
    {
        RegisteredWindow = window;
        IsWindowRegistered = true;

        var control = (Grid)RegisteredWindow!.Content;
        control.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;

        var resources = App.Current.Resources;
        Set(resources, "RawSystemAccentColor", (Color)resources["SystemAccentColor"]);

        if (!_settingsService.UseSystemAccentColor)
        {
            Set(resources, "SystemAccentColorLight1", _settingsService.CustomThemeColor.GetValueOrDefault());
            Set(resources, "SystemAccentColorLight2", _settingsService.CustomThemeColor.GetValueOrDefault());
            Set(resources, "SystemAccentColorLight3", _settingsService.CustomThemeColor.GetValueOrDefault());
            Set(resources, "SystemAccentColorDark1", _settingsService.CustomThemeColor.GetValueOrDefault());
            Set(resources, "SystemAccentColorDark2", _settingsService.CustomThemeColor.GetValueOrDefault());
            Set(resources, "SystemAccentColorDark3", _settingsService.CustomThemeColor.GetValueOrDefault());

            Set(resources, "SystemAccentColor", _settingsService.CustomThemeColor.GetValueOrDefault());
        }

        switch (_settingsService.BackgroundMode)
        {
            case 0:
                if (MicaController.IsSupported())
                    RegisteredWindow!.SystemBackdrop = new MicaBackdrop() { Kind = (MicaKind)_settingsService.MicaKind };
                control.Background = new SolidColorBrush(Colors.Transparent);
                break;
            case 1:
                if (DesktopAcrylicController.IsSupported())
                    RegisteredWindow!.SystemBackdrop = new DesktopAcrylicBackdrop(); 
                control.Background = new SolidColorBrush(Colors.Transparent);
                break;
            case 2:
                control.Background = _settingsService.SolidSelectedIndex == 0 || _settingsService.CustomBackgroundColor == null
                    ? App.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush
                    : new SolidColorBrush(_settingsService.CustomBackgroundColor.GetValueOrDefault(Colors.Transparent));
                break;
            case 3:
                if (File.Exists(_settingsService.ImageFilePath))
                {
                    try
                    {
                        using var fileStream = File.OpenRead(_settingsService.ImageFilePath);
                        using var randomAccessStream = fileStream.AsRandomAccessStream();

                        BitmapImage bitmapImage = new();
                        bitmapImage.SetSource(randomAccessStream);

                        lock (control)
                        {
                            ImageBrush imageBrush = new()
                            {
                                Stretch = Stretch.UniformToFill,
                                ImageSource = bitmapImage
                            };

                            control.Background = imageBrush;
                        }
                    }
                    catch { }
                }
                break;
        }

        if (_settingsService.BackgroundMode == 3 || _settingsService.BackgroundMode == 2)
        {
            Set(control.Resources, "NavigationViewContentBackground", new SolidColorBrush(Colors.Transparent));
            Set(control.Resources, "NavigationViewContentGridBorderThickness", new Thickness(0));
            Set(control.Resources, "BackgroundBorder", new Thickness(0));
        }
        else
        {
            Set(control.Resources, "NavigationViewContentBackground", App.Current.Resources["LayerFillColorDefaultBrush"]);
            Set(control.Resources, "NavigationViewContentGridBorderThickness", new Thickness(1, 1, 0, 0));
            Set(control.Resources, "BackgroundBorder", new Thickness(0, 1, 0, 0));
        }
    }

    public void RegisterApp(App app)
    {
        RegisteredApp = app;
        IsAppRegistered = true;
    }

    #region Settings Changed Events
    private void DisplayThemeChanged(global::FluentLauncher.Infra.Settings.SettingsContainer sender, global::FluentLauncher.Infra.Settings.SettingChangedEventArgs e)
    {
        if (IsWindowRegistered)
        {
            var element = ((FrameworkElement)RegisteredWindow!.Content);
            element.RequestedTheme = (ElementTheme)e.NewValue!;
        }
    }

    private void UseSystemAccentColorChanged(global::FluentLauncher.Infra.Settings.SettingsContainer sender, global::FluentLauncher.Infra.Settings.SettingChangedEventArgs e)
    {
        if (!IsAppRegistered) return;

        var resources = RegisteredApp!.Resources;

        if (e.NewValue is bool useSystemAccentColor && useSystemAccentColor)
        {
            var rawSystemAccentColor = (Color)resources["RawSystemAccentColor"];

            resources["SystemAccentColorLight1"] = rawSystemAccentColor;
            resources["SystemAccentColorLight2"] = rawSystemAccentColor;
            resources["SystemAccentColorLight3"] = rawSystemAccentColor;
            resources["SystemAccentColorDark1"] = rawSystemAccentColor;
            resources["SystemAccentColorDark2"] = rawSystemAccentColor;
            resources["SystemAccentColorDark3"] = rawSystemAccentColor;
            resources["SystemAccentColor"] = rawSystemAccentColor;
        } 
        else
        {
            resources["SystemAccentColorLight1"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            resources["SystemAccentColorLight2"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            resources["SystemAccentColorLight3"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            resources["SystemAccentColorDark1"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            resources["SystemAccentColorDark2"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            resources["SystemAccentColorDark3"] = _settingsService.CustomThemeColor.GetValueOrDefault();
            resources["SystemAccentColor"] = _settingsService.CustomThemeColor.GetValueOrDefault();
        }
    }

    private async void BackgroundModeChanged(global::FluentLauncher.Infra.Settings.SettingsContainer sender, global::FluentLauncher.Infra.Settings.SettingChangedEventArgs e)
    {
        if (!IsWindowRegistered) return;

        int backgroundMode = (int)e.NewValue!;
        var control = (Grid)RegisteredWindow!.Content;

        switch (backgroundMode)
        {
            case 0:
                if (MicaController.IsSupported())
                    RegisteredWindow!.SystemBackdrop = new MicaBackdrop() { Kind = (MicaKind)_settingsService.MicaKind };

                control.Background = new SolidColorBrush(Colors.Transparent);
                break;
            case 1:
                if (DesktopAcrylicController.IsSupported())
                    RegisteredWindow!.SystemBackdrop = new DesktopAcrylicBackdrop();

                control.Background = new SolidColorBrush(Colors.Transparent);
                break;
            case 2:
                control.Background = _settingsService.SolidSelectedIndex == 0 || _settingsService.CustomBackgroundColor == null
                    ? (Brush)App.Current.Resources["ApplicationPageBackgroundThemeBrush"]
                    : new SolidColorBrush(_settingsService.CustomBackgroundColor.GetValueOrDefault(Colors.Transparent));
                break;
            case 3:
                if (File.Exists(_settingsService.ImageFilePath))
                {
                    try
                    {
                        await Task.Delay(100);

                        using var fileStream = File.OpenRead(_settingsService.ImageFilePath);
                        using var randomAccessStream = fileStream.AsRandomAccessStream();

                        BitmapImage bitmapImage = new();
                        await bitmapImage.SetSourceAsync(randomAccessStream);

                        lock (control)
                        {
                            ImageBrush imageBrush = new()
                            {
                                Stretch = Stretch.UniformToFill,
                                ImageSource = bitmapImage
                            };

                            control.Background = imageBrush;
                        }
                    }
                    catch { }
                }
                break;
        }

        if (backgroundMode == 3 || backgroundMode == 2)
        {
            Set(control.Resources, "NavigationViewContentBackground", new SolidColorBrush(Colors.Transparent));
            Set(control.Resources, "NavigationViewContentGridBorderThickness", new Thickness(0));
            Set(control.Resources, "BackgroundBorder", new Thickness(0));
        } 
        else
        {
            Set(control.Resources, "NavigationViewContentBackground", App.Current.Resources["LayerFillColorDefaultBrush"]);
            Set(control.Resources, "NavigationViewContentGridBorderThickness", new Thickness(1, 1, 0, 0));
            Set(control.Resources, "BackgroundBorder", new Thickness(0, 1, 0, 0));
        }

        BackgroundReloaded?.Invoke(this, new());
    }

    private void PropertyChanged(global::FluentLauncher.Infra.Settings.SettingsContainer sender, global::FluentLauncher.Infra.Settings.SettingChangedEventArgs e)
        => BackgroundModeChanged(sender, new("BackgroundMode", _settingsService.BackgroundMode));
    #endregion

    private static void Set(ResourceDictionary keyValuePairs, string Key, object Value)
    {
        if (!keyValuePairs.ContainsKey(Key))
            keyValuePairs.Add(Key, Value);
        else keyValuePairs[Key] = Value;
    }
}
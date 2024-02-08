using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AppearanceViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentLanguage))]
    private string currentLanguage;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.NavigationViewDisplayMode))]
    private int navigationViewDisplayMode;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.DisplayTheme))]
    private int displayTheme;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.BackgroundMode))]
    private int backgroundMode;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseNewHomePage))]
    private bool useNewHomePage;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseSystemAccentColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColorString))] 
    private bool useSystemAccentColor;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableDefaultAcrylicBrush))]
    private bool enableDefaultAcrylicBrush;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.TintOpacity))]
    private double tintOpacity;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.TintLuminosityOpacity))]
    private double tintLuminosityOpacity;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ImageFilePath))]
    private string imageFilePath;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.SolidSelectedIndex))]
    private int solidSelectedIndex;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.SolidCustomColor))]
    private Color? solidCustomColor;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ThemeCustomColor))]
    [NotifyPropertyChangedFor(nameof(ThemeCustomColorString))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColorString))]
    private Color? themeCustomColor;

    public string ThemeCustomColorString => ThemeCustomColor != null 
        ? System.Drawing.ColorTranslator.ToHtml(
            System.Drawing.Color.FromArgb(
                CurrentThemeColor.A,
                CurrentThemeColor.R,
                CurrentThemeColor.G,
                CurrentThemeColor.B))
        : System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(255, default));

    public string CurrentThemeColorString 
    { 
        get 
        {
            return System.Drawing.ColorTranslator.ToHtml(
                System.Drawing.Color.FromArgb(
                    CurrentThemeColor.A, 
                    CurrentThemeColor.R, 
                    CurrentThemeColor.G, 
                    CurrentThemeColor.B));
        } 
    }

    public Color CurrentThemeColor => UseSystemAccentColor ? (Color)App.Current.Resources["RawSystemAccentColor"] : ThemeCustomColor.GetValueOrDefault();

    public List<string> SupportedLanguages => ResourceUtils.Languages;

    private Flyout backgroundColorFlyout;
    private Flyout themeColorFlyout;

    public AppearanceViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();

        PropertyChanged += AppearanceViewModel_PropertyChanged;
    }

    private void AppearanceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentLanguage))
            ResourceUtils.ApplyLanguage(CurrentLanguage);
    }

    [RelayCommand]
    private void SelectColorConfirm(Button button)
    {
        if (button.Tag.ToString() == "backgroundColor")
            backgroundColorFlyout.Hide();
        else themeColorFlyout.Hide();
    }

    [RelayCommand]
    private void Loaded(object args)
    {
        var button = args.As<Button, object>().sender;

        if (button.Tag.ToString() == "backgroundColor")
            backgroundColorFlyout = button.Flyout as Flyout;
        else themeColorFlyout = button.Flyout as Flyout;
    }
}

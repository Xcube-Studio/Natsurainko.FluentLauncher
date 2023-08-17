using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public List<string> SupportedLanguages => ResourceUtils.Languages;
    private Flyout ColorFlyout;

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
    private void SelectColorConfirm() => ColorFlyout.Hide();

    [RelayCommand]
    private void Loaded(object args)
    {
        var button = args.As<Button, object>().sender;
        ColorFlyout = button.Flyout as Flyout;
    }
}

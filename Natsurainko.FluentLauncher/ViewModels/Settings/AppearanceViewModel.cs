using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Settings;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AppearanceViewModel : SettingsPageVM<AppearancePage>, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;

    public AppearanceViewModel(SettingsService settingsService, NotificationService notificationService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.DisplayTheme))]
    public partial int DisplayTheme { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.BackgroundMode))]
    [NotifyPropertyChangedFor(nameof(CanUseImageThemeColor))]
    public partial int BackgroundMode { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseSystemAccentColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColor))]
    public partial bool UseSystemAccentColor { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MicaKind))]
    public partial int MicaKind { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ImageFilePath))]
    [NotifyPropertyChangedFor(nameof(ImageFileExists))]
    [NotifyPropertyChangedFor(nameof(CanUseImageThemeColor))]
    public partial string ImageFilePath { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.SolidSelectedIndex))]
    public partial int SolidSelectedIndex { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CustomThemeColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColor))]
    public partial Color? CustomThemeColor { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CustomBackgroundColor))]
    public partial Color? CustomBackgroundColor { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseBackgroundMask))]
    public partial bool UseBackgroundMask { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseHomeControlsMask))]
    public partial bool UseHomeControlsMask { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.HomeLaunchButtonSize))]
    public partial int HomeLaunchButtonSize { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableHomeLaunchTaskTrack))]
    public partial bool EnableHomeLaunchTaskTrack { get; set; }

    public Color CurrentThemeColor => UseSystemAccentColor ? (Color)App.Current.Resources["RawSystemAccentColor"] : CustomThemeColor.GetValueOrDefault();

    public bool AcrylicIsSupported => DesktopAcrylicController.IsSupported();

    public bool MicaIsSupported => MicaController.IsSupported();

    public bool ImageFileExists => File.Exists(ImageFilePath);

    public bool CanUseImageThemeColor => BackgroundMode == 3 && ImageFileExists;

    public void SetCustomThemeColor(Color color) => CustomThemeColor = color;

    public void SetCustomBackgroundColor(Color color) => CustomBackgroundColor = color;

    [RelayCommand]
    void HideFlyout(Flyout flyout) => flyout.Hide();

    [RelayCommand]
    void RadioButtonChecked(int index) => BackgroundMode = index;

    [RelayCommand]
    async Task UseImageThemeColor()
    {
        CustomThemeColor = await DominantColorHelper.GetColorFromImageAsync(ImageFilePath);
        var converter = (ColorHexCodeConverter)Application.Current.Resources["ColorHexCodeConverter"];

        _notificationService.NotifyWithoutContent(
            $"Image theme color successfully set, {converter.Convert(CustomThemeColor, null, null, null)}",
            icon: "\uE73E");
    }

    protected override void OnLoaded()
    {
        this.Page.ImageFileBox.QuerySubmitted += (_, _) =>
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Filter = "Png Image File|*.png|JPG Image File|*.jpg|BMP Image File|*.bmp|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
                ImageFilePath = openFileDialog.FileName;
        };
    }
}

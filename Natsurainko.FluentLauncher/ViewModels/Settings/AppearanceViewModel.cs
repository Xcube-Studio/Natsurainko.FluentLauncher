using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Views.Settings;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AppearanceViewModel : SettingsPageVM<AppearancePage>, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly INotificationService _notificationService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogActivationService;

    public AppearanceViewModel(
        SettingsService settingsService, 
        INotificationService notificationService,
        IDialogActivationService<ContentDialogResult> dialogActivationService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;
        _dialogActivationService = dialogActivationService;

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
    public partial string ImageFilePath { get; set; } = string.Empty;

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

    [RelayCommand]
    void HideFlyout(Flyout flyout) => flyout.Hide();

    [RelayCommand]
    void RadioButtonChecked(int index) => Dispatcher.TryEnqueue(() => BackgroundMode = index);

    [RelayCommand]
    async Task SelectThemeColor()
    {
        (ContentDialogResult result, Color color) = await _dialogActivationService.ShowAsync<Color>(
            "SelectColorDialog", CustomThemeColor!);

        if (result == ContentDialogResult.Primary)
        {
            CustomThemeColor = color;
            _notificationService.ThemeColorChanged(color);
        }
    }

    [RelayCommand]
    async Task SelectBackgroundColor()
    {
        (ContentDialogResult result, Color color) = await _dialogActivationService.ShowAsync<Color>(
            "SelectColorDialog", CustomBackgroundColor!);

        if (result == ContentDialogResult.Primary)
        {
            CustomBackgroundColor = color;
            _notificationService.ColorChanged(color);
        }
    }

    [RelayCommand]
    async Task UseImageThemeColor()
    {
        (ContentDialogResult result, Color color) = await _dialogActivationService.ShowAsync<Color>(
            "SelectImageThemeColorDialog", ImageFilePath);

        if (result == ContentDialogResult.Primary)
        {
            CustomThemeColor = color;
            _notificationService.ThemeColorChanged(color);
        }
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

internal static partial class AppearanceViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__ColorChanged", Message = "{colorHex}")]
    public static partial void ColorChanged(this INotificationService notificationService, string colorHex);

    public static void ColorChanged(this INotificationService notificationService, Color color)
        => ColorChanged(notificationService, System.Drawing.ColorTranslator.ToHtml(
                System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)));

    [Notification<InfoBar>(Title = "Notifications__ThemeColorChanged", Message = "{colorHex}")]
    public static partial void ThemeColorChanged(this INotificationService notificationService, string colorHex);

    public static void ThemeColorChanged(this INotificationService notificationService, Color color)
        => ThemeColorChanged(notificationService, System.Drawing.ColorTranslator.ToHtml(
                System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)));
}
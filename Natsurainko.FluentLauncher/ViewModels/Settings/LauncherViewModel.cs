using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class LauncherViewModel : SettingsPageVM, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly LocalStorageService _localStorageService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.HomePageLaunchButtonBehavior))]
    public partial int HomePageLaunchButtonBehavior { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.AfterInstanceLaunchedBehavior))]
    public partial int AfterInstanceLaunchedBehavior { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableLocalizedResourceSuggestions))]
    public partial bool EnableLocalizedResourceSuggestions { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxQuickLaunchLatestItem))]
    public partial int MaxQuickLaunchLatestItem { get; set; }

    public LauncherViewModel(SettingsService settingsService, LocalStorageService localStorageService)
    {
        _settingsService = settingsService;
        _localStorageService = localStorageService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [RelayCommand]
    void OpenStorageFolder() => ExplorerHelper.OpenFolder(LocalStorageService.LocalFolderPath);

    [RelayCommand]
    void OpenLogFolder() => ExplorerHelper.OpenFolder(_localStorageService.GetDirectory("launcher-logs").FullName);
}

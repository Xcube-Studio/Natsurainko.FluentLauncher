using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class DownloadViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly LocalStorageService _localStorageService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentDownloadSource))]
    public partial string CurrentDownloadSource { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxDownloadThreads))]
    public partial int MaxDownloadThreads { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableFragmentDownload))]
    public partial bool EnableFragmentDownload { get; set; }

    public string CoreConfigurationsFolder => _localStorageService.GetDirectory("GameConfigsFolder").FullName;

    public string LauncherCacheFolder => LocalStorageService.LocalFolderPath;


    public DownloadViewModel(SettingsService settingsService, LocalStorageService localStorageService)
    {
        _settingsService = settingsService;
        _localStorageService = localStorageService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [RelayCommand]
    public async Task OpenCacheFolder(string folder)
    {
        _ = await Launcher.LaunchFolderPathAsync(folder);
    }
}
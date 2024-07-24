using FluentLauncher.Infra.Settings.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Services.Storage;
using CommunityToolkit.Mvvm.Input;
using Windows.System;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class DownloadViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly LocalStorageService _localStorageService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentDownloadSource))]
    private string currentDownloadSource;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxDownloadThreads))]
    private int maxDownloadThreads;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableFragmentDownload))]
    private bool enableFragmentDownload;

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
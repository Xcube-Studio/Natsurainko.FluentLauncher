using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class DownloadViewModel : SettingsPageVM, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly LocalStorageService _localStorageService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxDownloadThreads))]
    public partial int MaxDownloadThreads { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableFragmentDownload))]
    public partial bool EnableFragmentDownload { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.FragmentDownloadWorkerCount))]
    public partial int FragmentDownloadWorkerCount { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxRetryCount))]
    public partial int MaxRetryCount { get; set; }

    [ObservableProperty]
    public partial int CurrentDownloadSource { get; set; }

    partial void OnCurrentDownloadSourceChanged(int value)
    {
        _settingsService.CurrentDownloadSource = CurrentDownloadSource switch
        {
            1 => "Bmclapi",
            _ => "Official"
        };
    }

    public DownloadViewModel(SettingsService settingsService, LocalStorageService localStorageService)
    {
        _settingsService = settingsService;
        _localStorageService = localStorageService;

        (this as ISettingsViewModel).InitializeSettings();

        CurrentDownloadSource = settingsService.CurrentDownloadSource switch
        {
            "Bmclapi" => 1,
            _ => 0
        };
    }
}
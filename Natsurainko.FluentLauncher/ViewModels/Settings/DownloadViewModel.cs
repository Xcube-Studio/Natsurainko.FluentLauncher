using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

partial class DownloadViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentDownloadSource))]
    private string currentDownloadSource;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxDownloadThreads))]
    private int maxDownloadThreads;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableFragmentDownload))]
    private bool enableFragmentDownload;


    public DownloadViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.Settings.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class LauncherViewModel : SettingsPageVM, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    public partial int AfterInstanceLaunched { get; set; }

    public LauncherViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        (this as ISettingsViewModel).InitializeSettings();
    }
}

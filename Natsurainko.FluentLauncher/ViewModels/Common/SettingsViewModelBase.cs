using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.Settings.Mvvm;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

abstract class SettingsViewModelBase : ObservableObject
{
    ~SettingsViewModelBase()
    {
        if (this is ISettingsViewModel settingsViewModel)
            settingsViewModel.RemoveSettingsChagnedHandlers();
    }
}

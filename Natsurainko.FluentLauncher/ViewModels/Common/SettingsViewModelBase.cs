using FluentLauncher.Infra.Settings.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

abstract class SettingsViewModelBase : ObservableObject
{
    ~SettingsViewModelBase()
    {
        if (this is ISettingsViewModel settingsViewModel)
            settingsViewModel.RemoveSettingsChagnedHandlers();
    }
}

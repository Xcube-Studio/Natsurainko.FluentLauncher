using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

abstract class SettingsViewModelBase : ObservableObject
{
    ~SettingsViewModelBase()
    {
        if (this is ISettingsViewModel settingsViewModel)
            settingsViewModel.RemoveSettingsChagnedHandlers();
    }
}

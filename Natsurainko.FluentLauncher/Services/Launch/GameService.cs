using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using Nrk.FluentCore.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Natsurainko.FluentLauncher.Components;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class GameService : DefaultGameService
{
    protected new SettingsService _settingsService;

    public GameService(SettingsService settingsService) : base(settingsService)
    {
        _settingsService = settingsService;
    }
}

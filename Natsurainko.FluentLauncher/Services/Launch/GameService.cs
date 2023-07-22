using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Services.Launch;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class GameService : DefaultGameService
{
    protected new SettingsService _settingsService;

    public GameService(SettingsService settingsService) : base(settingsService)
    {
        _settingsService = settingsService;
    }
}

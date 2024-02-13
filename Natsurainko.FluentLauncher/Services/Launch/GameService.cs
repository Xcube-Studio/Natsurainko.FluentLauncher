using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Services.Launch;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class GameService(SettingsService settingsService)
    : DefaultGameService(settingsService)
{
    public override void WhenActiveMinecraftFolderChanged(string? oldFolder, string? newFolder)
    {
        _settingsService.ActiveMinecraftFolder = newFolder;

        if (newFolder == null)
        {
            _games.Clear();
            return;
        }

        _locator = new GameLocator(newFolder);

        RefreshGames();
    }
}

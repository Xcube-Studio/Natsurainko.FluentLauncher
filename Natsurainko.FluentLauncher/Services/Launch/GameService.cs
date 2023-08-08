using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Services.Launch;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class GameService : DefaultGameService
{
    public new ReadOnlyObservableCollection<ExtendedGameInfo> GameInfos { get; }

    protected new readonly ObservableCollection<ExtendedGameInfo> _gameInfos;

    public new ExtendedGameInfo ActiveGameInfo { get; private set; }

    protected new GameLocator _locator;

    public GameService(SettingsService settingsService) : base()
    {
        _settingsService = settingsService;
        _minecraftFolders = settingsService.MinecraftFolders ?? new();
        _gameInfos = new();

        GameInfos = new(_gameInfos);
        MinecraftFolders = new(_minecraftFolders);

        if (!string.IsNullOrEmpty(_settingsService.ActiveMinecraftFolder) && MinecraftFolders.Contains(_settingsService.ActiveMinecraftFolder))
            ActivateMinecraftFolder(_settingsService.ActiveMinecraftFolder);
        else ActiveMinecraftFolder = null;
    }

    public override void ActivateMinecraftFolder(string folder)
    {
        if (!_minecraftFolders.Contains(folder))
            throw new ArgumentException("Not an folder managed by GameService", nameof(folder));

        if (ActiveMinecraftFolder != folder)
        {
            ActiveMinecraftFolder = folder;
            _settingsService.ActiveMinecraftFolder = folder;

            InitFolder();
        }
    }

    protected override void InitFolder()
    {
        //_versionsFolderWatcher?.Dispose();

        _locator = new GameLocator(ActiveMinecraftFolder);
        RefreshGames();

        //_versionsFolderWatcher = new FileSystemWatcher(ActiveMinecraftFolder);
    }

    protected override void RefreshGames()
    {
        _gameInfos.Clear();

        foreach (var game in _locator.EnumerateGames())
            _gameInfos.Add(game);

        if (_settingsService.ActiveGameInfo != null)
        {
            var extended = _settingsService.ActiveGameInfo.Extend();

            if (_gameInfos.Contains(extended))
                ActivateGameInfo(extended);
            else _settingsService.ActiveGameInfo = null;
        }
        else _settingsService.ActiveGameInfo = null;

        if (_settingsService.ActiveGameInfo == null)
            ActivateGameInfo(_gameInfos.First());
    }

    public void ActivateGameInfo(ExtendedGameInfo gameInfo)
    {
        if (!_gameInfos.Contains(gameInfo))
            throw new ArgumentException("Not an game managed by GameService", nameof(gameInfo));

        if (ActiveGameInfo != gameInfo)
        {
            ActiveGameInfo = gameInfo;
            _settingsService.ActiveGameInfo = gameInfo;
        }
    }
}

using Natsurainko.FluentLauncher.Services.Settings;
using System.Collections.ObjectModel;
using System;
using Nrk.FluentCore.Experimental.GameManagement.Instances;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class GameService
{
    #region Games 

    protected MinecraftInstance? _activeGame;
    public MinecraftInstance? ActiveGame
    {
        get => _activeGame;
        protected set
        {
            if (_activeGame != value)
                OnActiveInstanceChanged(_activeGame, value);

            _activeGame = value;
        }
    }

    protected ObservableCollection<MinecraftInstance> _games;
    public ReadOnlyObservableCollection<MinecraftInstance> Games { get; }

    #endregion

    #region Folders

    public string? _activeMinecraftFolder;
    public string? ActiveMinecraftFolder
    {
        get => _activeMinecraftFolder;
        protected set
        {
            if (_activeMinecraftFolder != value)
                OnActiveMinecraftFolderChanged(_activeMinecraftFolder, value);

            _activeMinecraftFolder = value;
        }
    }

    protected ObservableCollection<string> _minecraftFolders;
    public ReadOnlyObservableCollection<string> MinecraftFolders { get; }

    #endregion

    private readonly SettingsService _settingsService;

    private MinecraftInstanceParser? _instanceParser;

    public GameService(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _minecraftFolders = settingsService.MinecraftFolders ?? [];
        _games = [];

        Games = new(_games);
        MinecraftFolders = new(_minecraftFolders);

        ActivateMinecraftFolder(
            !string.IsNullOrEmpty(_settingsService.ActiveMinecraftFolder)
            && MinecraftFolders.Contains(_settingsService.ActiveMinecraftFolder)
            ? _settingsService.ActiveMinecraftFolder
            : null);
    }

    public virtual void OnActiveInstanceChanged(MinecraftInstance? oldGame, MinecraftInstance? newGame)
    {
        _settingsService.ActiveMinecraftInstance = newGame;
    }

    // From DefaultGameService
    //public virtual void WhenActiveMinecraftFolderChanged(string? oldFolder, string? newFolder)
    //{
    //    _settingsService.ActiveMinecraftFolder = newFolder;

    //    if (newFolder == null)
    //    {
    //        _games.Clear();
    //        ActivateGame(null);

    //        return;
    //    }

    //    _locator = new DefaultGameLocator(newFolder);

    //    //_versionsFolderWatcher?.Dispose(); 
    //    //_versionsFolderWatcher = new FileSystemWatcher(ActiveMinecraftFolder);

    //    RefreshGames();
    //}

    public virtual void ActivateMinecraftFolder(string? folder)
    {
        if (folder != null && !_minecraftFolders.Contains(folder))
            throw new ArgumentException("Not an folder managed by GameService", nameof(folder));

        ActiveMinecraftFolder = folder;
    }

    public virtual void ActivateGame(MinecraftInstance? MinecraftInstance)
    {
        if (MinecraftInstance != null && !_games.Contains(MinecraftInstance))
            throw new ArgumentException("Not an game managed by GameService", nameof(MinecraftInstance));

        ActiveGame = MinecraftInstance;
    }

    public virtual void RefreshGames()
    {
        _games.Clear();

        if (_instanceParser is null)
            throw new InvalidOperationException();

        foreach (var game in _instanceParser.ParseAllInstances())
            _games.Add(game);

        MinecraftInstance? MinecraftInstance = _settingsService.ActiveMinecraftInstance != null && _games.Contains(_settingsService.ActiveMinecraftInstance)
            ? _settingsService.ActiveMinecraftInstance
            : _games.Count > 0 ? _games[0] : null;

        ActivateGame(MinecraftInstance);
    }

    public virtual void AddMinecraftFolder(string folder)
    {
        _minecraftFolders.Add(folder);
        ActivateMinecraftFolder(folder);
    }

    public void RemoveMinecraftFolder(string folder)
    {
        _minecraftFolders.Remove(folder);

        if (ActiveMinecraftFolder == folder)
            ActivateMinecraftFolder(MinecraftFolders.Count > 0 ? MinecraftFolders[0] : null);
    }

    public void OnActiveMinecraftFolderChanged(string? oldFolder, string? newFolder)
    {
        _settingsService.ActiveMinecraftFolder = newFolder;

        if (newFolder == null)
        {
            _games.Clear();
            ActivateGame(null);

            return;
        }

        _instanceParser = new MinecraftInstanceParser(newFolder);

        RefreshGames();
    }
}

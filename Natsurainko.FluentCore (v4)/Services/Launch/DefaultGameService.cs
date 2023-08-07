using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.DefaultComponents.Launch;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nrk.FluentCore.Services.Launch;

/// <summary>
/// 游戏服务的默认实现（IoC适应）
/// </summary>
public class DefaultGameService
{
    public ReadOnlyObservableCollection<GameInfo> GameInfos { get; protected set; }

    protected ObservableCollection<GameInfo> _gameInfos;

    public ReadOnlyObservableCollection<string> MinecraftFolders { get; protected set; }

    protected ObservableCollection<string> _minecraftFolders;

    public string ActiveMinecraftFolder { get; protected set; }

    public GameInfo ActiveGameInfo { get; protected set; }

    protected DefaultGameLocator _locator;
    //protected FileSystemWatcher _versionsFolderWatcher; TODO: 文件监控实时更新GameInfos
    protected IFluentCoreSettingsService _settingsService;

    /// <summary>
    /// 以供完全自定义构造函数和重写该类
    /// </summary>
    public DefaultGameService()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settingsService">实际使用时请使用具体的继承类型替代之</param>
    public DefaultGameService(IFluentCoreSettingsService settingsService)
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

    public virtual void ActivateMinecraftFolder(string folder)
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

    public virtual void ActivateGameInfo(GameInfo gameInfo)
    {
        if (!_gameInfos.Contains(gameInfo))
            throw new ArgumentException("Not an game managed by GameService", nameof(gameInfo));

        if (ActiveGameInfo != gameInfo)
        {
            ActiveGameInfo = gameInfo;
            _settingsService.ActiveGameInfo = gameInfo;
        }
    }

    protected virtual void InitFolder()
    {
        //_versionsFolderWatcher?.Dispose();

        _locator = new DefaultGameLocator(ActiveMinecraftFolder);
        RefreshGames();

        //_versionsFolderWatcher = new FileSystemWatcher(ActiveMinecraftFolder);
    }

    protected virtual void RefreshGames()
    {
        _gameInfos.Clear();

        foreach (var game in _locator.EnumerateGames())
            _gameInfos.Add(game);

        if (_settingsService.ActiveGameInfo != null && _gameInfos.Contains(_settingsService.ActiveGameInfo))
            ActivateGameInfo(_settingsService.ActiveGameInfo);
        else
        {
            _settingsService.ActiveGameInfo = null;

            if (_gameInfos.Any())
                ActivateGameInfo(_gameInfos.First());
        }
    }
}

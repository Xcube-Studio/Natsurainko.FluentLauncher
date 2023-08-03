using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nrk.FluentCore.Components.Download;

/// <summary>
/// 资源下载器的抽象定义
/// </summary>
public abstract class BaseResourcesDownloader : IResourcesDownloader
{
    protected readonly GameInfo _gameInfo;
    protected IEnumerable<AssetElement> _assetElements;
    protected IEnumerable<LibraryElement> _libraryElements;

    public BaseResourcesDownloader(GameInfo gameInfo)
    {
        _gameInfo = gameInfo;
    }

    public event EventHandler SingleFileDownloaded;

    /// <summary>
    /// 设置要下载的依赖资源
    /// </summary>
    /// <param name="assetElements"></param>
    public abstract void SetAssetsElements(IEnumerable<AssetElement> assetElements);

    /// <summary>
    /// 设置要下载的依赖库
    /// </summary>
    /// <param name="libraryElements"></param>
    public abstract void SetLibraryElements(IEnumerable<LibraryElement> libraryElements);

    /// <summary>
    /// 下载并等待结束
    /// </summary>
    /// <param name="tokenSource"></param>
    public abstract void Download(CancellationTokenSource tokenSource = default);

    protected virtual void OnSingleFileDownloaded() => SingleFileDownloaded?.Invoke(this, EventArgs.Empty);
}

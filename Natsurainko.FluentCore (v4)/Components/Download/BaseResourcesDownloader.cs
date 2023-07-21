using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public abstract void SetAssetsElements(IEnumerable<AssetElement> assetElements);

    public abstract void SetLibraryElements(IEnumerable<LibraryElement> libraryElements);

    public abstract void Download();
}

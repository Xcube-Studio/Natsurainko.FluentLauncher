using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Components.Download;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.DefaultComponets.Download;

public class DefaultResoucresDownloader : BaseResourcesDownloader
{
    public DefaultResoucresDownloader(GameInfo gameInfo) : base(gameInfo) { }

    public override void Download()
    {

    }

    public override void SetAssetsElements(IEnumerable<AssetElement> assetElements)
    {
        _assetElements = assetElements;
    }

    public override void SetLibraryElements(IEnumerable<LibraryElement> libraryElements)
    {
        _libraryElements = libraryElements;
    }
}

using Nrk.FluentCore.Classes.Datas.Parse;
using System.Collections.Generic;

namespace Nrk.FluentCore.Classes.Data.Launch;

public record GameStatisticInfo
{
    public int LibrariesCount { get; set; }

    public int AssetsCount { get; set; }

    public long TotalSize { get; set; }

    public IEnumerable<ModLoaderInfo> ModLoaders { get; set; }
}

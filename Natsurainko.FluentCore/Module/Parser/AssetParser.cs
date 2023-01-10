using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Class.Model.Parser;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentCore.Module.Parser;

public class AssetParser
{
    public AssetManifestJsonEntity Entity { get; set; }

    public DirectoryInfo Root { get; set; }

    public AssetParser(AssetManifestJsonEntity jsonEntity, DirectoryInfo directoryInfo)
    {
        this.Entity = jsonEntity;
        this.Root = directoryInfo;
    }

    public IEnumerable<AssetResource> GetAssets()
    {
        foreach (var kvp in Entity.Objects)
            yield return new AssetResource
            {
                Name = kvp.Key,
                CheckSum = kvp.Value.Hash,
                Size = kvp.Value.Size,
                Root = this.Root
            };
    }
}

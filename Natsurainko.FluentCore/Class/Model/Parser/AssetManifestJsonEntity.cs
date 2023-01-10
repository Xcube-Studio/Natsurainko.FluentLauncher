using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Parser;

public class AssetManifestJsonEntity : IJsonEntity
{
    [JsonProperty("objects")]
    public Dictionary<string, AssetJsonEntity> Objects { get; set; }
}

public class AssetJsonEntity
{
    [JsonProperty("hash")]
    public string Hash { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }
}

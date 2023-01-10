using Newtonsoft.Json;

namespace Natsurainko.FluentCore.Class.Model.Install.Fabric;

public class FabricMavenItem
{
    [JsonProperty("separator")]
    public string Separator { get; set; }

    [JsonProperty("maven")]
    public string Maven { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
}

using Natsurainko.FluentCore.Class.Model.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Install.Fabric;

public class FabricInstallBuild
{
    [JsonProperty("intermediary")]
    public FabricMavenItem Intermediary { get; set; }

    [JsonProperty("loader")]
    public FabricMavenItem Loader { get; set; }

    [JsonProperty("launcherMeta")]
    public FabricLauncherMeta LauncherMeta { get; set; }
}

public class FabricLauncherMeta
{
    [JsonProperty("mainClass")]
    public JToken MainClass { get; set; }

    [JsonProperty("libraries")]
    public Dictionary<string, List<LibraryJsonEntity>> Libraries { get; set; }
}

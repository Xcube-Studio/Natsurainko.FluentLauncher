using Newtonsoft.Json;
using System;

namespace Natsurainko.FluentCore.Class.Model.Install.Forge;

public class ForgeInstallBuild
{
    [JsonProperty("branch")]
    public string Branch { get; set; }

    [JsonProperty("build")]
    public int Build { get; set; }

    [JsonProperty("mcversion")]
    public string McVersion { get; set; }

    [JsonProperty("version")]
    public string ForgeVersion { get; set; }

    [JsonProperty("modified")]
    public DateTime ModifiedTime { get; set; }
}

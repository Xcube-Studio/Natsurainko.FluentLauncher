using Newtonsoft.Json;

namespace Natsurainko.FluentCore.Class.Model.Install.OptiFine;

public class OptiFineInstallBuild
{
    [JsonProperty("patch")]
    public string Patch { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("mcversion")]
    public string McVersion { get; set; }

    [JsonProperty("filename")]
    public string FileName { get; set; }
}

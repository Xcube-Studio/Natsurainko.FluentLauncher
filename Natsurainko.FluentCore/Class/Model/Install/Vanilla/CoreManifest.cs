using Newtonsoft.Json;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Install.Vanilla;

public class CoreManifest
{
    [JsonProperty("latest")]
    public Dictionary<string, string> Latest { get; set; }

    [JsonProperty("versions")]
    public IEnumerable<CoreManifestItem> Cores { get; set; }
}

public class CoreManifestItem
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("time")]
    public string Time { get; set; }

    [JsonProperty("releaseTime")]
    public string ReleaseTime { get; set; }
}

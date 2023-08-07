using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Download;

public record VersionManifestJsonEntity
{
    [JsonPropertyName("latest")]
    public Dictionary<string, string> Latest { get; set; }

    [JsonPropertyName("versions")]
    public IEnumerable<VersionManifestItem> Versions { get; set; }
}

public record VersionManifestItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("releaseTime")]
    public string ReleaseTime { get; set; }
}
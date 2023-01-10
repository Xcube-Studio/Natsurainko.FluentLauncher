using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Mod;

public class CurseForgeModpack
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("summary")]
    public string Description { get; set; }

    [JsonProperty("links")]
    public Dictionary<string, string> Links { get; set; }

    [JsonProperty("downloadCount")]
    public int DownloadCount { get; set; }

    [JsonProperty("dateModified")]
    public DateTime LastUpdateTime { get; set; }

    [JsonProperty("gamePopularityRank")]
    public int GamePopularityRank { get; set; }

    [JsonProperty("latestFilesIndexes")]
    public List<CurseForgeModpackFileInfo> LatestFilesIndexes { get; set; }

    [JsonProperty("categories")]
    public List<CurseForgeModpackCategory> Categories { get; set; }

    public string IconUrl { get; set; }

    public Dictionary<string, List<CurseForgeModpackFileInfo>> Files { get; set; } = new();

    public string[] SupportedVersions { get; set; }
}

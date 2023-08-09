using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Download;

public record CurseResource
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("classId")]
    public int ClassId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("downloadCount")]
    public int DownloadCount { get; set; }

    [JsonPropertyName("dateModified")]
    public DateTime DateModified { get; set; }

    [JsonPropertyName("latestFilesIndexes")]
    public IEnumerable<CurseFile> Files { get; set; }

    public IEnumerable<string> Categories { get; set; }

    public IEnumerable<string> Authors { get; set; }

    public IEnumerable<string> ScreenshotUrls { get; set; }

    public string WebLink { get; set; }

    public string IconUrl { get; set; }
}

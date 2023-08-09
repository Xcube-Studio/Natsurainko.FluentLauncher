using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Download;

public record ModrinthResource
{
    [JsonPropertyName("project_id")]
    public string Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("project_type")]
    public string ProjectType { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Summary { get; set; }

    [JsonPropertyName("downloads")]
    public int DownloadCount { get; set; }

    [JsonPropertyName("date_modified")]
    public DateTime DateModified { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("display_categories")]
    public IEnumerable<string> Categories { get; set; }

    [JsonPropertyName("gallery")]
    public IEnumerable<string> ScreenshotUrls { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    public string WebLink => $"https://modrinth.com/{ProjectType}/{Slug}";
}

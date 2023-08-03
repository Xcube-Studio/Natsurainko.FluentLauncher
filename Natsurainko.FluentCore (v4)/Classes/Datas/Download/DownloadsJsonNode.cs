using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Download;

public record DownloadsJsonNode
{
    [JsonPropertyName("artifact")]
    public FileJsonNode Artifact { get; set; }

    [JsonPropertyName("classifiers")]
    public Dictionary<string, FileJsonNode> Classifiers { get; set; }

}

public record FileJsonNode
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    //for client-x.xx.xml
    [JsonPropertyName("id")]
    public string Id { get; set; }
}


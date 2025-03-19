using System.Text.Json.Serialization;

#nullable disable
namespace Natsurainko.FluentLauncher.Models;

internal record NewsData
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("tag")]
    public string Tag { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("readMoreLink")]
    public string ReadMoreUrl { get; set; }

    public string ImageUrl { get; set; }

    public string[] Tags { get; set; }
}

internal record PatchNoteData
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("shortText")]
    public string ShortText { get; set; }

    [JsonPropertyName("contentPath")]
    public string ContentPath { get; set; }

    public string ImageUrl { get; set; }
}
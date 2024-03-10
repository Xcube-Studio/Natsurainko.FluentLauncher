using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Natsurainko.FluentLauncher.Models.UI;

internal class NewsData
{
    public string? ImageUrl { get; set; } = null!;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("readMoreLink")]
    public string? ReadMoreUrl { get; set; }


    public static NewsData Deserialize(JsonNode node)
    {
        NewsData? data = node.Deserialize<NewsData>();
        if (data is null)
            throw new JsonException("Failed to deserialize news data");

        string urlPath = node["newsPageImage"]?["url"]?.GetValue<string>()
            ?? throw new JsonException("Failed to get news image URL");
        data.ImageUrl = $"https://launchercontent.mojang.com{urlPath}";

        return data;
    }
}

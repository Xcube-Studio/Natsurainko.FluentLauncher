using System.Text.Json.Serialization;

namespace Natsurainko.FluentLauncher.Classes.Data.UI;

internal class NewsData
{
    public string ImageUrl { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("tag")]
    public string Tag { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("readMoreLink")]
    public string ReadMoreUrl { get; set; }
}

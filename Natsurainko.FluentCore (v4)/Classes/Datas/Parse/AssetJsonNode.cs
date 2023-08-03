using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Parse;

public record AssetJsonNode
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}

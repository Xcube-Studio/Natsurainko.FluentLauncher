using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nrk.FluentCore.Classes.Datas.Parse;

/// <summary>
/// version.json 对应的实体类
/// </summary>
public record VersionJsonEntity
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; }

    [JsonPropertyName("minecraftArguments")]
    public string MinecraftArguments { get; set; }

    [JsonPropertyName("inheritsFrom")]
    public string InheritsFrom { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("assets")]
    public string Assets { get; set; }

    [JsonPropertyName("arguments")]
    public ArgumentsJsonNode Arguments { get; set; }

    [JsonPropertyName("assetIndex")]
    public AssstIndexJsonNode AssetIndex { get; set; }
}

/// <summary>
/// version.json 下 arguments 键 对应的实体类
/// </summary>
public class ArgumentsJsonNode
{
    [JsonPropertyName("game")]
    public IEnumerable<JsonElement> Game { get; set; }

    [JsonPropertyName("jvm")]
    public IEnumerable<JsonElement> Jvm { get; set; }
}

/// <summary>
/// version.json 下 assetIndex 键 对应的实体类
/// </summary>
public class AssstIndexJsonNode
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }
}
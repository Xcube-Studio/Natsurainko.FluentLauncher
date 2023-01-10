using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Parser;

public class VersionJsonEntity : IJsonEntity
{
    [JsonProperty("arguments")]
    public ArgumentsJsonEntity Arguments { get; set; }

    [JsonProperty("assetIndex")]
    public AssetIndexJsonEntity AssetIndex { get; set; }

    [JsonProperty("assets")]
    public string Assets { get; set; }

    [JsonProperty("javaVersion")]
    public JavaVersionJsonEntity JavaVersion { get; set; } = new JavaVersionJsonEntity() { MajorVersion = 8 };

    [JsonProperty("downloads")]
    public Dictionary<string, FileJsonEntity> Downloads { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("libraries")]
    public List<LibraryJsonEntity> Libraries { get; set; }

    [JsonProperty("logging")]
    public LoggingJsonEntity Logging { get; set; }

    [JsonProperty("minecraftArguments")]
    public string MinecraftArguments { get; set; }

    [JsonProperty("mainClass")]
    public string MainClass { get; set; }

    [JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }

    [JsonProperty("jar")]
    public string Jar { get; set; }

    [JsonProperty("minimumLauncherVersion")]
    public int? MinimumLauncherVersion { get; set; }

    [JsonProperty("releaseTime")]
    public string ReleaseTime { get; set; }

    [JsonProperty("time")]
    public string Time { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}

public class LoggingJsonEntity
{
    [JsonProperty("client")]
    public ClientJsonEntity Client { get; set; }
}

public class ArgumentsJsonEntity
{
    [JsonProperty("game")]
    public List<JToken> Game { get; set; }

    [JsonProperty("jvm")]
    public List<JToken> Jvm { get; set; }
}

public class JavaVersionJsonEntity
{
    [JsonProperty("component")]
    public string Component { get; set; }

    [JsonProperty("majorVersion")]
    public int MajorVersion { get; set; }
}

public class FileJsonEntity
{
    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("sha1")]
    public string Sha1 { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    //for client-x.xx.xml
    [JsonProperty("id")]
    public string Id { get; set; }
}

public class AssetIndexJsonEntity : FileJsonEntity
{
    [JsonProperty("totalSize")]
    public int TotalSize { get; set; }
}

public class ClientJsonEntity
{
    [JsonProperty("argument")]
    public string Argument { get; set; }

    [JsonProperty("file")]
    public FileJsonEntity File { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}

public class LibraryJsonEntity
{
    [JsonProperty("downloads")]
    public DownloadsJsonEntity Downloads { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    // 为旧式Forge Library提供
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("natives")]
    public Dictionary<string, string> Natives { get; set; }

    [JsonProperty("rules")]
    public IEnumerable<RuleModel> Rules { get; set; }

    // 为旧式Forge Library提供
    [JsonProperty("checksums")]
    public List<string> CheckSums { get; set; }

    // 为旧式Forge Library提供
    [JsonProperty("clientreq")]
    public bool? ClientReq { get; set; }
}

public class DownloadsJsonEntity
{
    [JsonProperty("artifact")]
    public FileJsonEntity Artifact { get; set; }

    [JsonProperty("classifiers")]
    public Dictionary<string, FileJsonEntity> Classifiers { get; set; }
}

public class RuleModel
{
    [JsonProperty("action")]
    public string Action { get; set; }


    [JsonProperty("os")]
    public Dictionary<string, string> System { get; set; }
}

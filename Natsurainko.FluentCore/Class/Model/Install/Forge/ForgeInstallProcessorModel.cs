using Newtonsoft.Json;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Install.Forge;

public class ForgeInstallProcessorModel
{
    [JsonProperty("sides")]
    public List<string> Sides { get; set; } = new List<string>();

    [JsonProperty("jar")]
    public string Jar { get; set; }

    [JsonProperty("classpath")]
    public List<string> Classpath { get; set; }

    [JsonProperty("args")]
    public List<string> Args { get; set; }

    [JsonProperty("outputs")]
    public Dictionary<string, string> Outputs { get; set; } = new();
}

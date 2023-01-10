using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Auth.Microsoft;

public class MicrosoftAuthenticationResponse : IJsonEntity
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("skins")]
    public List<SkinModel> Skins { get; set; }

    [JsonProperty("capes")]
    public JArray Capes { get; set; }
}

public class SkinModel
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("variant")]
    public string Variant { get; set; }

    [JsonProperty("alias")]
    public string Alias { get; set; }
}

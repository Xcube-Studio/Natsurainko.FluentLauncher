using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Auth.Microsoft;

public class DisplayClaimsModel
{
    [JsonProperty("xui")]
    public List<JObject> Xui { get; set; }
}

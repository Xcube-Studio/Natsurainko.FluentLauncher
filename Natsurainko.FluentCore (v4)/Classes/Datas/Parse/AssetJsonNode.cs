using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Datas.Parse;

public record AssetJsonNode
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}

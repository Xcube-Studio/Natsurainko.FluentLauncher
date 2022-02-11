using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model
{
    /// <summary>
    /// 版本清单模型
    /// </summary>
    public class VersionManifestModel
    {
        [JsonProperty("latest")]
        public Dictionary<string, string> Latest { get; set; }

        [JsonProperty("versions")]
        public IEnumerable<VersionManifestItem> Versions { get; set; }
    }
}

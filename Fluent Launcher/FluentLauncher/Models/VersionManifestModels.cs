using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentLauncher.Models
{
    public class VersionManifestModel
    {
        [JsonProperty("latest")]
        public Dictionary<string, string> Latest { get; set; }

        [JsonProperty("versions")]
        public IEnumerable<VersionManifestItem> Versions { get; set; }
    }

    public class VersionManifestItem
    {
        public string Icon
        {
            get
            {
                if (Type.Contains("old_beta") || Type.Contains("old_alpha"))
                    return "Dirt_Podzol";
                if (Type.Contains("snapshot"))
                    return "Crafting_Table";

                return "Grass";
            }
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("releaseTime")]
        public string ReleaseTime { get; set; }
    }

}

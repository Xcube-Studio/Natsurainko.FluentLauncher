using Newtonsoft.Json;

namespace FluentCore.Model
{
    /// <summary>
    /// 版本清单子项模型
    /// </summary>
    public class VersionManifestItem
    {
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

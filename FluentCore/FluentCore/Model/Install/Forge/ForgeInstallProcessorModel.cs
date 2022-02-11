using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model.Install.Forge
{
    /// <summary>
    /// Forge安装进程模型
    /// </summary>
    public class ForgeInstallProcessorModel
    {
        [JsonProperty("sides")]
        public List<string> Sides { get; set; }

        [JsonProperty("jar")]
        public string Jar { get; set; }

        [JsonProperty("classpath")]
        public List<string> Classpath { get; set; }

        [JsonProperty("args")]
        public List<string> Args { get; set; }

        [JsonProperty("outputs")]
        public Dictionary<string, string> Outputs { get; set; }
    }
}

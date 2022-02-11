using Newtonsoft.Json;

namespace FluentCore.Model.Install.Forge
{
    /// <summary>
    /// Forge安装数据模型
    /// <para>
    /// 1.13+
    /// </para>
    /// </summary>
    public class ForgeInstallDataModel
    {
        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }
    }
}

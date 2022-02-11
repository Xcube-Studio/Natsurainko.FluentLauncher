using Newtonsoft.Json;

namespace FluentCore.Model.Auth
{
    /// <summary>
    /// 基础请求模型
    /// </summary>
    public abstract class BaseRequestModel
    {
        /// <summary>
        /// 客户端标识符
        /// </summary>
        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }
    }

}

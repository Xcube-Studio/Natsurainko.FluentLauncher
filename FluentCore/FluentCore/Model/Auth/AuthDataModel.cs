using System;

namespace FluentCore.Model.Auth
{
    /// <summary>
    /// 验证数据模型
    /// </summary>
    public class AuthDataModel
    {
        /// <summary>
        /// 玩家的uuid
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// 玩家的验证令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 玩家的实际用户名
        /// </summary>
        public string UserName { get; set; }
    }
}

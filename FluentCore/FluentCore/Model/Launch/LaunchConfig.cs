using FluentCore.Model.Auth;
using System;

namespace FluentCore.Model.Launch
{
    /// <summary>
    /// 启动配置信息
    /// </summary>
    public class LaunchConfig
    {
        /// <summary>
        /// Java可执行文件路径
        /// </summary>
        public string JavaPath { get; set; }

        /// <summary>
        /// Natives文件存放位置
        /// </summary>
        public string NativesFolder { get; set; }

        /// <summary>
        /// 最大设定内存
        /// </summary>
        public int MaximumMemory { get; set; } = 1024;

        /// <summary>
        /// 最小设定内存
        /// </summary>
        public int? MinimumMemory { get; set; } = 512;

        /// <summary>
        /// 额外前置参数
        /// </summary>
        public string MoreFrontArgs { get; set; } = default;

        /// <summary>
        /// 额外后置参数
        /// </summary>
        public string MoreBehindArgs { get; set; } = default;

        /// <summary>
        /// 客户端令牌
        /// </summary>
        public string ClientToken { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 验证数据模型
        /// </summary>
        public AuthDataModel AuthDataModel { get; set; }
    }
}

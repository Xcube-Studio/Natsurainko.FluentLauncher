using System.Collections.Generic;

namespace FluentCore.Model.Launch
{
    /// <summary>
    /// 启动结果
    /// </summary>
    public class LaunchResult
    {
        /// <summary>
        /// 游戏输出日志
        /// </summary>
        public IEnumerable<string> Logs { get; set; }

        /// <summary>
        /// 游戏错误输出日志
        /// </summary>
        public IEnumerable<string> Errors { get; set; }

        /// <summary>
        /// 游戏启动参数
        /// </summary>
        public string Args { get; set; }

        /// <summary>
        /// 游戏是否崩溃
        /// </summary>
        public bool IsCrashed { get; set; }
    }
}

using System;

namespace FluentCore.Exceptions.Launcher
{
    /// <summary>
    /// 游戏核心未找到的错误
    /// </summary>
    public class GameCoreNotFoundException : Exception
    {
        /// <summary>
        /// 游戏核心所对应的id
        /// </summary>
        public string Id { get; set; }

        public new string Message { get; set; } = "找不到游戏核心";
    }
}

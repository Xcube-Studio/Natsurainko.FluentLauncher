using FluentCore.Service.Local;
using System;

namespace FluentCore.Exceptions.Launcher
{
    /// <summary>
    /// 游戏进程已经运行的错误
    /// </summary>
    public class GameHasRanException : Exception
    {
        /// <summary>
        /// 游戏进程
        /// </summary>
        public ProcessContainer ProcessContainer { get; set; }

        public new string Message { get; set; } = "游戏进程已经运行";
    }
}

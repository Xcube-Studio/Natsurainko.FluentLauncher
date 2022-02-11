using System;

namespace FluentCore.Event.Process
{
    /// <summary>
    /// 进程退出事件参数
    /// </summary>
    public class ProcessExitedEventArgs : EventArgs
    {
        /// <summary>
        /// 进程运行总时间
        /// </summary>
        public TimeSpan RunTime { get; set; }

        /// <summary>
        /// 进程退出id
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// 进程是否正常退出
        /// </summary>
        public bool IsNormal { get; set; }
    }
}

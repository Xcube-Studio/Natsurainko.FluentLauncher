using System;
using System.Collections.Generic;

namespace FluentCore.Event.Process
{
    /// <summary>
    /// 进程崩溃事件参数
    /// </summary>
    public class ProcessCrashedEventArgs : EventArgs
    {
        /// <summary>
        /// 崩溃日志集
        /// </summary>
        public IEnumerable<string> CrashData { get; set; }
    }
}

using FluentCore.Model;
using System;

namespace FluentCore.Event.Process
{
    /// <summary>
    /// 进程状态改变事件参数
    /// </summary>
    public class ProcessStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 进程状态
        /// </summary>
        public ProcessState ProcessState { get; set; }
    }
}

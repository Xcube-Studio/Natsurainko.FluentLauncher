namespace FluentCore.Model
{
    /// <summary>
    /// 进程状态
    /// </summary>
    public enum ProcessState
    {
        Undefined = 0,

        /// <summary>
        /// 已初始化
        /// </summary>
        Initialized = 1,

        /// <summary>
        /// 运行时
        /// </summary>
        Running = 2,

        /// <summary>
        /// 已挂起
        /// </summary>
        Suspended = 3,

        /// <summary>
        /// 未响应
        /// </summary>
        Unresponding = 4,

        /// <summary>
        /// 已退出
        /// </summary>
        Exited = 5
    }
}

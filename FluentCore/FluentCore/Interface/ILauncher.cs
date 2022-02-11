using FluentCore.Model.Launch;
using FluentCore.Service.Local;
using System;

namespace FluentCore.Interface
{
    /// <summary>
    /// 启动器封装接口
    /// </summary>
    public interface ILauncher : IDisposable
    {
        /// <summary>
        /// 游戏进程
        /// </summary>
        ProcessContainer ProcessContainer { get; }

        /// <summary>
        /// 启动配置
        /// </summary>
        LaunchConfig LaunchConfig { get; }

        /// <summary>
        /// 游戏核心定位器
        /// </summary>
        ICoreLocator CoreLocator { get; set; }

        /// <summary>
        /// 根据id启动指定的游戏
        /// </summary>
        /// <param name="id"></param>
        void Launch(string id);

        /// <summary>
        /// 立即终止游戏
        /// </summary>
        void Stop();
    }
}

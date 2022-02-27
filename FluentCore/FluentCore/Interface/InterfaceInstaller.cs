using FluentCore.Event;
using FluentCore.Service.Component.Launch;
using System;

namespace FluentCore.Interface
{
    /// <summary>
    /// 安装器接口
    /// </summary>
    public interface InterfaceInstaller
    {
        /// <summary>
        /// 游戏核心定位器
        /// </summary>
        CoreLocator CoreLocator { get; set; }

        /// <summary>
        /// 安装进度改变事件
        /// </summary>
        public event EventHandler<InstallerProgressChangedEventArgs> ProgressChanged;
    }
}

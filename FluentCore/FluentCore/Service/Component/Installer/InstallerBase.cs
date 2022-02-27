using FluentCore.Event;
using FluentCore.Interface;
using FluentCore.Service.Component.Launch;
using System;

namespace FluentCore.Service.Component.Installer
{
    /// <summary>
    /// 基础游戏安装器
    /// </summary>
    public class InstallerBase : InterfaceInstaller
    {
        /// <summary>
        /// 游戏核心定位器
        /// </summary>
        public CoreLocator CoreLocator { get; set; }

        public InstallerBase(CoreLocator locator) => this.CoreLocator = locator;

        public event EventHandler<InstallerProgressChangedEventArgs> ProgressChanged;

        protected virtual void OnProgressChanged(double progress, string content) => ProgressChanged?.Invoke(this, new InstallerProgressChangedEventArgs
        {
            Progress = progress,
            StepName = content
        });
    }
}

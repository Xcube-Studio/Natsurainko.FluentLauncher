using FluentCore.Interface;
using FluentCore.Service.Component.Launch;

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
    }
}

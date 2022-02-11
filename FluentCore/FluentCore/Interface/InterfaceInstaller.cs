using FluentCore.Service.Component.Launch;

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
    }
}

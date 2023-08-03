namespace Nrk.FluentCore.Classes.Datas.Launch;

/// <summary>
/// 用于记录游戏的基本信息和信息源
/// </summary>
public record GameInfo
{
    /// <summary>
    /// 游戏的显示名称（昵称）
    /// <para>
    /// 若实现的GameLocator不支持此功能，应默认返回 <see cref="AbsoluteId"/>
    /// </para>
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 游戏的绝对Id
    /// <para>
    /// 即 version.json 中的 "Id"
    /// </para>
    /// </summary>
    public string AbsoluteId { get; set; }

    /// <summary>
    /// 游戏核心的绝对版本号（如果能获取）
    /// </summary>
    public string AbsoluteVersion { get; set; }

    /// <summary>
    /// 游戏核心的类型，可取的值 "release","snapshot","old_beta","old_alpha"
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 游戏是否为原版
    /// </summary>
    public bool IsVanilla { get; set; }

    /// <summary>
    /// 是否有继承的核心
    /// </summary>
    public bool IsInheritedFrom { get; set; }

    /// <summary>
    /// 继承的核心，若 <see cref="IsInheritedFrom"/> = <see langword="false"/>，应返回 <see langword="null"/>
    /// </summary>
    public GameInfo InheritsFrom { get; set; }

    /// <summary>
    /// .minecraft 目录的绝对路径
    /// </summary>
    public string MinecraftFolderPath { get; set; }

    /// <summary>
    /// version.json 的绝对路径
    /// </summary>
    public string VersionJsonPath { get; set; }

    /// <summary>
    /// version.jar 的绝对路径（如果存在）
    /// </summary>
    public string JarPath { get; set; }

    /// <summary>
    /// assets\indexes\assetindex.json 的绝对路径（如果存在）
    /// </summary>
    public string AssetsIndexJsonPath { get; set; }
}

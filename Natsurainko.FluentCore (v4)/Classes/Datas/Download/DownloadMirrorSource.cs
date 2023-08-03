using System.Collections.Generic;

namespace Nrk.FluentCore.Classes.Datas.Download;

/// <summary>
/// 表示一个下载镜像源
/// </summary>
public record DownloadMirrorSource
{
    /// <summary>
    /// 镜像源域名
    /// </summary>
    public required string Domain { get; set; }

    /// <summary>
    /// 标识名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 版本清单获取地址
    /// </summary>
    public required string VersionManifestUrl { get; set; }

    /// <summary>
    /// 依赖库地址替换字典
    /// </summary>
    public required Dictionary<string, string> LibrariesReplaceUrl { get; set; }

    /// <summary>
    /// 依赖材质地址替换字典
    /// </summary>
    public required Dictionary<string, string> AssetsReplaceUrl { get; set; }
}

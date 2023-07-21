using Nrk.FluentCore.Interfaces;

namespace Nrk.FluentCore.Classes.Datas.Parse;

/// <summary>
/// 表示一个依赖库文件
/// </summary>
public record LibraryElement : IDownloadElement
{
    /// <summary>
    /// 是否为本地依赖库
    /// </summary>
    public bool IsNativeLibrary { get; set; }

    /// <summary>
    /// 相对于 libraries 文件夹路径
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// 绝对路径
    /// </summary>
    public string AbsolutePath { get; set; }

    /// <summary>
    /// 下载地址（如果可能）
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 校验码（如果可能）
    /// </summary>
    public string Checksum { get; set; }
}

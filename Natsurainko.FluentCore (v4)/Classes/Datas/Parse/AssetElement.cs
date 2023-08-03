using Nrk.FluentCore.Interfaces;

namespace Nrk.FluentCore.Classes.Datas.Parse;

/// <summary>
/// 表示一个依赖材质文件
/// </summary>
public record AssetElement : IDownloadElement
{
    /// <summary>
    /// 对应依赖文件的原始文件名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 相对于 assets 文件夹路径
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// 绝对路径
    /// </summary>
    public string AbsolutePath { get; set; }

    /// <summary>
    /// 下载地址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 校验码
    /// </summary>
    public string Checksum { get; set; }
}

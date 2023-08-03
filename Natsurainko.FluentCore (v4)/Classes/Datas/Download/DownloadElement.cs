using Nrk.FluentCore.Interfaces;

namespace Nrk.FluentCore.Classes.Datas.Download;

/// <summary>
/// 表示一个下载元素
/// </summary>
public class DownloadElement : IDownloadElement
{
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

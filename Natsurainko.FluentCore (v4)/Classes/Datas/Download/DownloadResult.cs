using Nrk.FluentCore.Interfaces;
using System;

namespace Nrk.FluentCore.Classes.Datas.Download;

/// <summary>
/// 表示下载任务结果
/// </summary>
public class DownloadResult
{
    /// <summary>
    /// 是否失败
    /// </summary>
    public bool IsFaulted { get; set; }

    /// <summary>
    /// 失败的下载元素，若未失败该值为null
    /// </summary>
    public IDownloadElement DownloadElement { get; set; }

    /// <summary>
    /// 导致失败的异常，若未失败该值为null
    /// </summary>
    public Exception Exception { get; set; }
}

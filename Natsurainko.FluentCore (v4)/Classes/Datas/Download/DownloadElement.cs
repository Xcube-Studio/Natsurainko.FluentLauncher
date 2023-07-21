using Nrk.FluentCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Datas.Download;

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

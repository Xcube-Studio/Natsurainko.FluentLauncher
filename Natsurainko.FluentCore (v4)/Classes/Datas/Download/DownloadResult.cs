using Nrk.FluentCore.Interfaces;
using System;

namespace Nrk.FluentCore.Classes.Datas.Download;

public class DownloadResult
{
    public bool IsFaulted { get; set; }

    public IDownloadElement DownloadElement { get; set; }

    public Exception Exception { get; set; }
}

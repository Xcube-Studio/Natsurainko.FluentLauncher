using System;
using System.Threading;

namespace Nrk.FluentCore.Interfaces;

/// <summary>
/// 资源下载器接口
/// </summary>
public interface IResourcesDownloader
{
    public event EventHandler SingleFileDownloaded;

    public void Download(CancellationTokenSource tokenSource = default);
}

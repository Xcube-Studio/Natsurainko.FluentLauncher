using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Interfaces;

/// <summary>
/// 资源下载器接口
/// </summary>
public interface IResourcesDownloader
{
    public event EventHandler SingleFileDownloaded;

    public void Download(CancellationTokenSource tokenSource = default);
}

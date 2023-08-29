using Nrk.FluentCore.DefaultComponents.Download;
using System;
using System.IO;
using System.Text;

namespace Natsurainko.FluentLauncher.Classes.Exceptions;

public class CompleteGameResourcesException : Exception
{
    public CompleteGameResourcesException(DefaultResourcesDownloader resourcesDownloader) 
        : base("补全游戏依赖资源失败")
    {
        var @string = new StringBuilder();

        foreach (var resource in resourcesDownloader.ErrorDownload)
        {
            @string.AppendLine(Path.GetFileName(resource.DownloadElement.AbsolutePath) + " 下载失败");
        }
    }
}

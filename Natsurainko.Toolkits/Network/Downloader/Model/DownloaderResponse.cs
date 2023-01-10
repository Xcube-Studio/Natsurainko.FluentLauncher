using System;
using System.IO;

namespace Natsurainko.Toolkits.Network.Downloader.Model;

public class DownloaderResponse
{
    public DownloaderCompletionType CompletionType { get; set; }

    public TimeSpan DownloadTime { get; set; }

    public Exception Exception { get; set; }
}

public class DownloaderResponse<TResult> : DownloaderResponse
{
    public TResult Result { get; set; }
}

public class SimpleDownloaderResponse : DownloaderResponse<FileInfo>
{
    public bool Success { get; set; }
}
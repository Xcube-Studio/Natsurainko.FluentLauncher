namespace Nrk.FluentCore.Classes.Datas.Download;

public record DownloadSetting
{
    public long FileSizeThreshold { get; set; }

    public bool EnableLargeFileMultiPartDownload { get; set; }

    public int MultiPartsCount { get; set; }

    public int MultiThreadsCount { get; set; }
}

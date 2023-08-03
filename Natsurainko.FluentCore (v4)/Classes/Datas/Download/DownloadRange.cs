namespace Nrk.FluentCore.Classes.Datas.Download;

/// <summary>
/// 表示一个分片文件的下载范围
/// </summary>
public class DownloadRange
{
    public long Start { get; set; }

    public long End { get; set; }

    public string TempFileAbsolutePath { get; set; }
}

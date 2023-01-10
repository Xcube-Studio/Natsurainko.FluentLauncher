using Natsurainko.FluentCore.Interface;
using Natsurainko.Toolkits.Network.Model;
using System.IO;

namespace Natsurainko.FluentCore.Class.Model.Download;

public class FileResource : IResource
{
    public DirectoryInfo Root { get; set; }

    public string Name { get; set; }

    public int Size { get; set; }

    public string CheckSum { get; set; }

    public string Url { get; set; }

    public FileInfo FileInfo { get; set; }

    public HttpDownloadRequest ToDownloadRequest()
        => new()
        {
            Directory = this.FileInfo.Directory,
            FileName = this.Name,
            Sha1 = this.CheckSum,
            Size = this.Size,
            Url = this.Url
        };

    public FileInfo ToFileInfo() => this.FileInfo;
}

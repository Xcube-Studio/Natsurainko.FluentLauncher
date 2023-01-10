using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Service;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using System.IO;

namespace Natsurainko.FluentCore.Class.Model.Download;

public class AssetResource : IResource
{
    public DirectoryInfo Root { get; set; }

    public string Name { get; set; }

    public int Size { get; set; }

    public string CheckSum { get; set; }

    public HttpDownloadRequest ToDownloadRequest()
        => new()
        {
            Directory = this.ToFileInfo().Directory,
            FileName = this.CheckSum,
            Sha1 = this.CheckSum,
            Size = this.Size,
            Url = UrlExtension.Combine(
                DownloadApiManager.Current.Assets,
                this.CheckSum.Substring(0, 2),
                this.CheckSum)
        };

    public FileInfo ToFileInfo()
        => new(Path.Combine(
            this.Root.FullName,
            "assets",
            "objects",
            this.CheckSum.Substring(0, 2),
            this.CheckSum));
}

using Natsurainko.Toolkits.Network.Model;
using System.IO;

namespace Natsurainko.FluentCore.Interface;

public interface IResource
{
    DirectoryInfo Root { get; set; }

    string Name { get; }

    int Size { get; }

    string CheckSum { get; }

    FileInfo ToFileInfo();

    HttpDownloadRequest ToDownloadRequest();
}

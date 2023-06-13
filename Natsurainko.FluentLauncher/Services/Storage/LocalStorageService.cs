using Natsurainko.FluentLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Storage;

class LocalStorageService
{
    /// <summary>
    /// Full path of the local app data store
    /// </summary>
    public static string LocalFolderPath { get; }

    // Consider making LocalFolderPath a non-static member and creating a separate class for non-msix packaged app
    static LocalStorageService()
    {
        if (MsixPackageUtils.IsPackaged)
            LocalFolderPath = ApplicationData.Current.LocalFolder.Path; // This takes a long time when stepping over in debugging, but looks ok without a breakpoint.
        else
            throw new NotSupportedException();
    }

    /// <summary>
    /// Determines whether the file at the given path exists in the local app data store
    /// </summary>
    /// <param name="path">Path of the file in the local app data store</param>
    /// <returns>true if path referts to an existing file</returns>
    public bool HasFile(string path)
        => File.Exists(Path.Combine(LocalFolderPath, path));

    /// <summary>
    /// Determines whether the directory at the given path exists in the local app data store
    /// </summary>
    /// <param name="path">Path of the directory in the local app data store</param>
    /// <returns>true if path referts to an existing directory</returns>
    public bool HasDirectory(string path)
        => Directory.Exists(Path.Combine(Path.GetDirectoryName(path), path));

    /// <summary>
    /// Get a file in the local app data store. <br/>
    /// This does not guarantee that the file exists.
    /// </summary>
    /// <param name="path">Path of the file in the local app data folder</param>
    /// <returns>FileInfo object of the file</returns>
    public FileInfo GetFile(string path)
    {
        string fullPath = Path.Combine(LocalFolderPath, path);

        return new FileInfo(fullPath);
    }

    /// <summary>
    /// Get a directory in the local app data store. <br/>
    /// Creates the directory if not exist.
    /// </summary>
    /// <param name="path">Path of the directory in the local app data folder</param>
    /// <returns>DirectoryInfo object of the directory</returns>
    public DirectoryInfo GetDirectory(string path)
    {
        string fullPath = Path.Combine(LocalFolderPath, path);

        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        return new DirectoryInfo(fullPath);
    }

}

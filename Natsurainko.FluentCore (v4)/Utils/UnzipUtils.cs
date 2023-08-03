using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Nrk.FluentCore.Utils;

public static class UnzipUtils
{
    public static void BatchUnzip(string targetFolder, IEnumerable<string> files)
    {
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        DirectoryExtensions.DeleteAllFiles(targetFolder);

        var extension = EnvironmentUtils.PlatformName switch
        {
            "windows" => ".dll",
            "linux" => ".so",
            "osx" => ".dylib",
            _ => "."
        };

        foreach (var file in files)
        {
            using ZipArchive zip = ZipFile.OpenRead(file);

            foreach (ZipArchiveEntry entry in zip.Entries)
                if (Path.GetExtension(entry.Name).Contains(extension))
                    entry.ExtractToFile(Path.Combine(targetFolder, entry.Name), true);
        }
    }
}

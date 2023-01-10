using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.Toolkits.IO;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Natsurainko.FluentCore.Module.Launcher;

public class NativesDecompressor
{
    public static void Decompress(DirectoryInfo directory, IEnumerable<LibraryResource> libraryResources)
    {
        if (!directory.Exists)
            directory.Create();

        directory.DeleteAllFiles();

        foreach (var item in libraryResources.Where(x => x.IsEnable && x.IsNatives))
            using (ZipArchive zip = ZipFile.OpenRead(item.ToFileInfo().FullName))
                foreach (ZipArchiveEntry entry in zip.Entries)
                    if (Path.GetExtension(entry.Name).Contains(".dll") || Path.GetExtension(entry.Name).Contains(".so") || Path.GetExtension(entry.Name).Contains(".dylib"))
                        entry.ExtractToFile(Path.Combine(directory.FullName, entry.Name), true);
    }
}

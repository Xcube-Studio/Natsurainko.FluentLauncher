using FluentCore.Model.Game;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FluentCore.Service.Local
{
    public class NativesDecompressor
    {
        public NativesDecompressor(string root, string id)
        {
            this.Root = root;
            this.Id = id;
        }

        public string Root { get; set; }

        public string Id { get; set; }

        public void Decompress(IEnumerable<Native> natives, string nativesFolder = default)
        {
            nativesFolder = string.IsNullOrEmpty(nativesFolder) ? $"{PathHelper.GetVersionFolder(Root, Id)}{PathHelper.X}natives" : nativesFolder;

            if (!Directory.Exists(nativesFolder))
                Directory.CreateDirectory(nativesFolder);

            FileHelper.DeleteAllFiles(new DirectoryInfo(nativesFolder));

            foreach (var item in natives)
                using (ZipArchive zip = ZipFile.OpenRead($"{PathHelper.GetLibrariesFolder(Root)}{PathHelper.X}{item.GetRelativePath()}"))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (Path.GetExtension(entry.Name).Contains(".dll") || Path.GetExtension(entry.Name).Contains(".so") || Path.GetExtension(entry.Name).Contains(".dylib"))
                            entry.ExtractToFile($"{nativesFolder}{PathHelper.X}{entry.Name}", true);
        }
    }
}

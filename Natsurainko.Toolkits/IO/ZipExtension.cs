using System.IO;
using System.IO.Compression;

namespace Natsurainko.Toolkits.IO
{
    public static class ZipExtension
    {
        public static string GetString(this ZipArchiveEntry zipArchiveEntry)
        {
            using var stream = zipArchiveEntry.Open();
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static void ExtractTo(this ZipArchiveEntry zipArchiveEntry, string filename)
        {
            var file = new FileInfo(filename);

            if (!file.Directory.Exists)
                file.Directory.Create();

            zipArchiveEntry.ExtractToFile(filename, true);
        }
    }
}

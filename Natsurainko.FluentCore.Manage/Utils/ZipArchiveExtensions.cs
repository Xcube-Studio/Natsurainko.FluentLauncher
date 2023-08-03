using System.IO;
using System.IO.Compression;

namespace Nrk.FluentCore.Utils;

public static class ZipArchiveExtensions
{
    public static string ReadAsString(this ZipArchiveEntry archiveEntry)
    {
        using var stream = archiveEntry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

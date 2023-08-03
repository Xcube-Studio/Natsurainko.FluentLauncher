using System.Collections.Generic;
using System.IO;

namespace Nrk.FluentCore.Utils;

internal static class DirectoryExtensions
{
    public static void DeleteAllFiles(string folder) => DeleteAllFiles(new DirectoryInfo(folder));

    public static void DeleteAllFiles(this DirectoryInfo directory)
    {
        foreach (FileInfo file in directory.EnumerateFiles())
            file.Delete();

        foreach (var item in directory.EnumerateDirectories())
        {
            DeleteAllFiles(item);
            item.Delete();
        }
    }

    public static IEnumerable<FileInfo> FindAll(this DirectoryInfo directory, string file)
    {
        foreach (var item in directory.EnumerateFiles())
            if (item.Name == file)
                yield return item;

        foreach (var item in directory.EnumerateDirectories())
            foreach (var info in item.FindAll(file))
                yield return info;
    }
}

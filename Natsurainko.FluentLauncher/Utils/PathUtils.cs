using System;
using System.IO;

namespace Natsurainko.FluentLauncher.Utils;

internal static partial class PathUtils
{
    public static bool IsValidPath(string path, bool isFile = false)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
            return false;

        char[] invalidPathChars = Path.GetInvalidPathChars();
        char[] invalidFileNameChars = Path.GetInvalidPathChars();

        foreach (char c in path)
            if (Array.Exists(invalidPathChars, element => element == c))
                return false;

        if (isFile)
        {
            try
            {
                foreach (char c in Path.GetFileName(path))
                    if (Array.Exists(invalidFileNameChars, element => element == c))
                        return false;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }
}

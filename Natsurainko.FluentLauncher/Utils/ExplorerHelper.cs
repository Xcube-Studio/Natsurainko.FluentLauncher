using System.Diagnostics;

namespace Natsurainko.FluentLauncher.Utils;

internal static partial class ExplorerHelper
{
    public static void ShowAndSelectFile(string? filePath)
    {
        if (filePath == null)
            return;

        using var process = Process.Start("explorer.exe", $"/select,{filePath}");
    }

    public static void OpenFolder(string? folderPath)
    {
        if (folderPath == null)
            return;

        using var process = Process.Start("explorer.exe", $"/root,{folderPath}");
    }
}

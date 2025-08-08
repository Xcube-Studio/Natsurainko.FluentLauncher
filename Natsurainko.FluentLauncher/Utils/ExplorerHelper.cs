using System.Diagnostics;

namespace Natsurainko.FluentLauncher.Utils;

internal static class ExplorerHelper
{
    public static void ShowAndSelectFile(string filePath)
    {
        using var process = Process.Start("explorer.exe", $"/select,{filePath}");
    }
}

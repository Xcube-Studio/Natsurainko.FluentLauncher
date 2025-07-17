using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class FileInfoExtensions
{
    public const int OF_READWRITE = 2;
    public const int OF_SHARE_DENY_NONE = 0x40;
    public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

    [DllImport("kernel32.dll")]
    public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);

    public static bool IsFileOccupied(this FileInfo fileInfo)
    {
        if (!fileInfo.Exists)
            throw new InvalidOperationException("File Not Exists");

        IntPtr vHandle = _lopen(fileInfo.FullName, OF_READWRITE | OF_SHARE_DENY_NONE);
        var result = vHandle == HFILE_ERROR;
        CloseHandle(vHandle);

        return result;
    }

    public static bool TryParse(string filePath, [NotNullWhen(true)] out FileInfo? fileInfo)
    {
        try
        {
            fileInfo = new FileInfo(filePath);
            return true;
        }
        catch (Exception)
        {
            fileInfo = null;
            return false;
        }
    }
}

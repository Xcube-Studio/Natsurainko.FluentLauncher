using System;
using System.Runtime.InteropServices;

namespace Nrk.FluentCore.Utils;

/// <summary>
/// 环境信息工具
/// </summary>
internal class EnvironmentUtils
{
    public static readonly string PlatformName = GetPlatformName();
    public static readonly string SystemArch = GetSystemArch();

    /// <summary>
    /// 获取当前平台名称
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "osx";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "windows";

        return "unknown";
    }

    /// <summary>
    /// 获取系统位数
    /// </summary>
    /// <returns></returns>
    public static string GetSystemArch() => Environment.Is64BitOperatingSystem ? "64" : "32";
}

using System;
using System.Runtime.InteropServices;

namespace Natsurainko.Toolkits.Values
{
    public static class EnvironmentInfo
    {
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

        public static string Arch => Environment.Is64BitOperatingSystem ? "64" : "32";
    }
}

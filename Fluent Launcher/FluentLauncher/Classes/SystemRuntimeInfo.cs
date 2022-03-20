using Microsoft.Win32;
using System;

namespace FluentLauncher.Classes
{
    public static class SystemRuntimeInfo
    {
        public static string OperatingSystemInfo()
        {
            using var key = Registry.LocalMachine;
            using var system = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
            string displayVersion = (string)system.GetValue("DisplayVersion");

            return $"Windows {Environment.OSVersion.Version.Major} {displayVersion} {Environment.OSVersion.Version.Build}";
        }
    }
}

using Microsoft.Win32;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Natsurainko.FluentCore.Extension.Windows.Service
{
    [SupportedOSPlatform("windows")]
    public class JavaHelper
    {
        public static IEnumerable<string> SearchJavaRuntime(IEnumerable<string> others = null)
        {
            var result = new List<string>();

            #region Cmd

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true,
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            var output = new List<string>();

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => output.Add(e.Data);
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => output.Add(e.Data);

            process.StandardInput.WriteLine("where javaw");
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();

            output.Where(x => string.IsNullOrEmpty(x) || x.Contains('>')).ToList().ForEach(x => output.Remove(x));

            if (output.Any())
                result.AddNotRepeating(output.Skip(2));

            process.Dispose();

            #endregion

            #region Regedit

            var javaHomePaths = new List<string>();

            List<string> ForRegistryKey(RegistryKey registryKey, string keyName)
            {
                var result = new List<string>();

                foreach (string valueName in registryKey.GetValueNames())
                    if (valueName == keyName)
                        result.Add((string)registryKey.GetValue(valueName));

                foreach (string registrySubKey in registryKey.GetSubKeyNames())
                    ForRegistryKey(registryKey.OpenSubKey(registrySubKey), keyName).ForEach(x => result.Add(x));

                return result;
            };

            using var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE");

            if (reg.GetSubKeyNames().Contains("JavaSoft"))
            {
                using var registryKey = reg.OpenSubKey("JavaSoft");
                ForRegistryKey(registryKey, "JavaHome").ForEach(x => javaHomePaths.Add(x));
            }

            if (reg.GetSubKeyNames().Contains("WOW6432Node"))
            {
                using var registryKey = reg.OpenSubKey("WOW6432Node");
                if (registryKey.GetSubKeyNames().Contains("JavaSoft"))
                {
                    using var registrySubKey = reg.OpenSubKey("JavaSoft");
                    ForRegistryKey(registrySubKey, "JavaHome").ForEach(x => javaHomePaths.Add(x));
                }
            }

            foreach (var item in javaHomePaths)
                if (Directory.Exists(item))
                    result.AddNotRepeating(new DirectoryInfo(item).FindAll("javaw.exe").Select(x => x.FullName));

            #endregion

            #region Special Folders

            var folders = new List<string>()
            {
                Path.Combine(Environment.GetEnvironmentVariable("APPDATA"),".minecraft\\cache\\java"),
                Environment.GetEnvironmentVariable("JAVA_HOME"),
                "C:\\Program Files\\Java"
            };

            foreach (var item in folders)
                if (Directory.Exists(item))
                    result.AddNotRepeating(new DirectoryInfo(item).FindAll("javaw.exe").Select(x => x.FullName));

            if (others != null)
                foreach (var item in others)
                    if (Directory.Exists(item))
                        result.AddNotRepeating(new DirectoryInfo(item).FindAll("javaw.exe").Select(x => x.FullName));
            #endregion

            return result;
        }
    }
}

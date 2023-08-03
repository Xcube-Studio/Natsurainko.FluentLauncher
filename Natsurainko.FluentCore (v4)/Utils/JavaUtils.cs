using Microsoft.Win32;
using Nrk.FluentCore.Classes.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Nrk.FluentCore.Utils;

[SupportedOSPlatform("windows")]
public static class JavaUtils
{
    public static IEnumerable<string> SearchJava(IEnumerable<string> others = null)
    {
        var result = new List<string>();

        #region Cmd

        using var process = new Process()
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

        result.AddRange(output.Where(x => !string.IsNullOrEmpty(x) && x.EndsWith("javaw.exe") && File.Exists(x)));

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
                result.AddRange(new DirectoryInfo(item).FindAll("javaw.exe").Select(x => x.FullName));

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
                result.AddRange(new DirectoryInfo(item).FindAll("javaw.exe").Select(x => x.FullName));

        if (others != null)
            foreach (var item in others)
                if (Directory.Exists(item))
                    result.AddRange(new DirectoryInfo(item).FindAll("javaw.exe").Select(x => x.FullName));
        #endregion

        return result.Distinct();
    }

    public static JavaInfo GetJavaInfo(string file)
    {
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(file);
        var name = fileVersionInfo.ProductName.Split(" ")[0];

        if (fileVersionInfo.ProductName.StartsWith("Java(TM)"))
            name = $"Java {fileVersionInfo.ProductMajorPart}";
        else if (fileVersionInfo.ProductName.StartsWith("OpenJDK"))
            name = $"OpenJDK {fileVersionInfo.ProductMajorPart}";

        var runtimeInfo = new JavaInfo
        {
            Name = name,
            ProductName = fileVersionInfo.ProductName,
            Company = fileVersionInfo.CompanyName,
            Version = new Version(
                fileVersionInfo.ProductMajorPart,
                fileVersionInfo.ProductMinorPart,
                fileVersionInfo.ProductBuildPart,
                fileVersionInfo.ProductPrivatePart),
            Architecture = GetPeArchitecture(file) switch
            {
                523 => "x64",
                267 => "x86",
                _ => "unknown"
            }
        };

        return runtimeInfo;
    }

    public static ushort GetPeArchitecture(string filePath)
    {
        ushort architecture = 0;

        try
        {
            using var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var bReader = new BinaryReader(fStream);

            if (bReader.ReadUInt16() == 23117)
            {
                fStream.Seek(0x3A, SeekOrigin.Current);
                fStream.Seek(bReader.ReadUInt32(), SeekOrigin.Begin);

                if (bReader.ReadUInt32() == 17744)
                {
                    fStream.Seek(20, SeekOrigin.Current);
                    architecture = bReader.ReadUInt16();
                }
            }
        }
        catch { }

        return architecture;
    }
}

using Nrk.FluentCore.Classes.Datas;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Nrk.FluentCore.Utils;

public static class MemoryUtils
{
    [SupportedOSPlatform("windows")]
    public static MemoryMetrics GetWindowsMetrics()
    {
        using var process = Process.Start(new ProcessStartInfo()
        {
            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        });

        process.WaitForExit();

        var lines = process.StandardOutput.ReadToEnd().Trim().Split("\n");
        var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

        var total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
        var free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);

        return new MemoryMetrics
        {
            Total = total,
            Free = free,
            Used = total - free
        };
    }

    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("osx")]
    public static MemoryMetrics GetUnixMetrics()
    {
        using var process = Process.Start(new ProcessStartInfo("free -m")
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput = true
        });

        process.WaitForExit();

        var lines = process.StandardOutput.ReadToEnd().Split("\n");
        var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

        return new MemoryMetrics
        {
            Total = double.Parse(memory[1]),
            Used = double.Parse(memory[2]),
            Free = double.Parse(memory[3])
        };
    }

    public static (int, int) CalculateJavaMemory(int min = 512)
    {
#pragma warning disable CA1416
        var metrics = EnvironmentUtils.PlatformName.Equals("windows")
            ? GetWindowsMetrics()
            : GetUnixMetrics();
#pragma warning restore CA1416

        var willUsed = metrics.Free * 0.6;
        var max = willUsed < min ? min : Convert.ToInt32(willUsed);

        return (max, min);
    }
}

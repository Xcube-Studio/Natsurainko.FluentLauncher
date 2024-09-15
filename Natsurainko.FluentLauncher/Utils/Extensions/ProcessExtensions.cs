using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class ProcessExtensions
{
    public static string? GetCommandLine(this Process process)
    {
        string? cmdLine = null;
        using (var searcher = new ManagementObjectSearcher(
          $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
        {
            // By definition, the query returns at most 1 match, because the process 
            // is looked up by ID (which is unique by definition).
            using var matchEnum = searcher.Get().GetEnumerator();

            if (matchEnum.MoveNext()) // Move to the 1st item.
                cmdLine = matchEnum.Current["CommandLine"]?.ToString();
        }
        if (cmdLine == null)
        {
            // Not having found a command line implies 1 of 2 exceptions, which the
            // WMI query masked:
            // An "Access denied" exception due to lack of privileges.
            // A "Cannot process request because the process (<pid>) has exited."
            // exception due to the process having terminated.
            // We provoke the same exception again simply by accessing process.MainModule.
            var dummy = process.MainModule; // Provoke exception.
        }
        return cmdLine;
    }

    public static void KillProcessTree(this Process process)
    {
        static void KillProcessTreeByPid(int pid)
        {
            using var managementObjectSearcher = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={pid}");
            using var managementBaseObjects = managementObjectSearcher.Get();

            foreach (var managementObject in managementBaseObjects)
                KillProcessTreeByPid(Convert.ToInt32(managementObject["ProcessID"]));

            try
            {
                using Process process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch { /* */ }
        }

        if (process.Id == 0)
            throw new InvalidOperationException("Cannot Close System Idle Process");

        try
        {
            KillProcessTreeByPid(process.Id);
            process.Kill();
        }
        catch { /* */ }
    }
}

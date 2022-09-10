using Natsurainko.FluentLauncher.Shared.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Desktop;

public class CurrentApplication
{
    public static DesktopServiceListener DesktopService { get; set; }

    public static string StorageFolder => ApplicationData.Current.LocalFolder.Path;

    public static readonly List<Type> Modules = new string[]
    {
        "Application",
        "GameCoreLocator",
        "JavaHelper",
        "Application",
        "DefaultSettings",
        "DownloaderProcess",
        "LauncherProcess",
        "LocalModManager"
    }
    .Select(module => Type.GetType($"Natsurainko.FluentLauncher.Shared.Mapping.{module}"))
    .ToList();

    public static void Main(string[] args)
    {
        if (Debugger.IsAttached)
            AllocConsole();

        ServicePointManager.DefaultConnectionLimit = 1024;

        DesktopService = new DesktopServiceListener();
        DesktopService.BindingModules(Modules);
        DesktopService.InitializeAsync().Wait();

        DesktopService.WaitForExited();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();
}
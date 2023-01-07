using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Downloader;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.Toolkits.Network.Downloader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace Natsurainko.FluentLauncher.Components;

public class WorkingProcessEntryPoint
{
    [DllImport("Kernel32.dll")]
    public static extern void AllocConsole();

    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for System.CommandLine");

        var folder = new Option<string>(name: "--folder");
        var core = new Option<string>(name: "--core");
        var threadNumber = new Option<int>(name: "--thread-number", getDefaultValue: () => 128);

        var downloader = new Command("downloader")
        {
            folder,
            core,
            threadNumber
        };

        rootCommand.Add(downloader);
        downloader.SetHandler(Downloader, folder, core, threadNumber);

        return rootCommand.Invoke(args);
    }

    static void Downloader(string folder, string core, int threadNumber)
    {
        ResourceDownloader.MaxDownloadThreads = threadNumber;

        var locator = new GameCoreLocator(folder);
        var downloader = new ResourceDownloader(locator.GetGameCore(core));
        downloader.DownloadProgressChanged += (_, e) => Console.WriteLine(JsonConvert.SerializeObject(e));

        Console.WriteLine(JsonConvert.SerializeObject(downloader.Download(), new FileInfoJsonConverter(), new DirectoryInfoJsonConverter()));
    }
}

public class CrossProcessResourceDownload : IResourceDownloader
{
    public Process Process { get; private set; }

    public GameCore GameCore { get; set; }

    public event EventHandler<ParallelDownloaderProgressChangedEventArgs> DownloadProgressChanged;

    public CrossProcessResourceDownload() { }

    public CrossProcessResourceDownload(GameCore core)
    {
        GameCore = core;
    }

    public ParallelDownloaderResponse Download()
        => DownloadAsync().GetAwaiter().GetResult();

    public async Task<ParallelDownloaderResponse> DownloadAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = this.GetType().Assembly.Location.Replace(".dll", ".exe"),
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = $"downloader " +
                $"--folder {GameCore.Root.FullName} " +
                $"--core {GameCore.Id} " +
                $"--thread-number {ResourceDownloader.MaxDownloadThreads}"
        };

        var outputs = new List<string>();
        var errors = new List<string>();
        Process = new Process { StartInfo = startInfo };

        Process.ErrorDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            outputs.Add(e.Data);
            errors.Add(e.Data);
        };
        Process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data) || !e.Data.StartsWith("{"))
                return;

            outputs.Add(e.Data);
            DownloadProgressChanged?.Invoke(this, JsonConvert.DeserializeObject<ParallelDownloaderProgressChangedEventArgs>(e.Data));
        };

        Process.Start();
        Process.BeginErrorReadLine();
        Process.BeginOutputReadLine();

        await Process.WaitForExitAsync();

        var last = outputs.Last();

        if (Process.ExitCode == 0)
            return JsonConvert.DeserializeObject<ParallelDownloaderResponse>(last, new FileInfoJsonConverter(), new DirectoryInfoJsonConverter());
        else return new ParallelDownloaderResponse() { Exception = new Exception(string.Join("\r\n", errors)) };
    }
}
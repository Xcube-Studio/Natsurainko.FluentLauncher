using Natsurainko.FluentCore.Module.Downloader;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentCore.Service;
using Natsurainko.FluentLauncher.Utils;
using Newtonsoft.Json;
using System;
using System.CommandLine;

namespace Natsurainko.FluentLauncher.Components.CrossProcess;

public class WorkingProcessEntryPoint
{
    public static Command DownloaderCommand
    {
        get
        {
            var folder = new Option<string>(name: "--folder");
            var core = new Option<string>(name: "--core");
            var threadNumber = new Option<int>(name: "--thread-number", getDefaultValue: () => 128);
            var source = new Option<string>(name: "--source", getDefaultValue: () => "mcbbs");

            var downloader = new Command("downloader")
            {
                folder,
                core,
                threadNumber,
                source
            };

            downloader.SetHandler(Downloader, folder, core, threadNumber, source);

            return downloader;
        }
    }

    static void Downloader(string folder, string core, int threadNumber, string source)
    {
        ResourceDownloader.MaxDownloadThreads = threadNumber;
        DownloadApiManager.Current = source switch
        {
            "Mcbbs" => DownloadApiManager.Mcbbs,
            "Bmclapi" => DownloadApiManager.Bmcl,
            "Mojang" => DownloadApiManager.Mojang,
            _ => DownloadApiManager.Mcbbs
        };

        var locator = new GameCoreLocator(folder);
        var downloader = new ResourceDownloader(locator.GetGameCore(core));
        downloader.DownloadProgressChanged += (_, e) => Console.WriteLine(JsonConvert.SerializeObject(e));

        Console.WriteLine(JsonConvert.SerializeObject(downloader.Download(), new FileInfoJsonConverter(), new DirectoryInfoJsonConverter()));
    }

    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Natsurainko.FluentLauncher Working Process EntryPoint");
        rootCommand.Add(DownloaderCommand);

        return rootCommand.Invoke(args);
    }
}
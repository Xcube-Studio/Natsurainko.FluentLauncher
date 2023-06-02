using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.Toolkits.Network.Downloader;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components.CrossProcess;

public class CrossProcessResourceDownloader : IResourceDownloader
{
    public Process Process { get; private set; }

    public IGameCore GameCore { get; set; }

    public event EventHandler<ParallelDownloaderProgressChangedEventArgs> DownloadProgressChanged;

    private int _maxDownloadThreads;

    public CrossProcessResourceDownloader(SettingsService settings) 
    {
        _maxDownloadThreads = settings.MaxDownloadThreads;
    }

    public CrossProcessResourceDownloader(IGameCore core, SettingsService settings) : this(settings)
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
                $"--folder {GameCore.Root.FullName.ToPath()} " +
                $"--core {GameCore.Id} " +
                $"--thread-number {_maxDownloadThreads}"
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
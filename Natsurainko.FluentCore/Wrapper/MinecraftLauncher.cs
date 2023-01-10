using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Launcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Wrapper;

public class MinecraftLauncher : ILauncher
{
    public LaunchSetting LaunchSetting { get; private set; }

    public ArgumentsBuilder ArgumentsBuilder { get; private set; }

    public IAuthenticator Authenticator { get; set; }

    public IGameCoreLocator GameCoreLocator { get; set; }

    public IResourceDownloader ResourceDownloader { get; set; }

    public MinecraftLauncher(LaunchSetting launchSetting, IGameCoreLocator gameCoreLocator)
    {
        this.LaunchSetting = launchSetting;
        this.GameCoreLocator = gameCoreLocator;

        if (this.LaunchSetting.Account == null)
            throw new ArgumentNullException("LaunchSetting.Account");
    }

    public MinecraftLauncher(LaunchSetting launchSetting, IAuthenticator authenticator, IGameCoreLocator gameCoreLocator)
    {
        this.LaunchSetting = launchSetting;
        this.Authenticator = authenticator;
        this.GameCoreLocator = gameCoreLocator;
    }

    public LaunchResponse LaunchMinecraft(string id)
        => this.LaunchMinecraftAsync(id).GetAwaiter().GetResult();

    public async Task<LaunchResponse> LaunchMinecraftAsync(string id)
    {
        IEnumerable<string> args = new string[] { };
        Process process = null;

        try
        {
            var core = this.GameCoreLocator.GetGameCore(id);
            if (core == null)
                return new LaunchResponse(null, LaunchState.Failed, null);

            if (this.ResourceDownloader != null)
            {
                this.ResourceDownloader.GameCore = core;
                await this.ResourceDownloader.DownloadAsync();
            }

            if (this.Authenticator != null)
                this.LaunchSetting.Account = await this.Authenticator.AuthenticateAsync();

            this.ArgumentsBuilder = new ArgumentsBuilder(core, this.LaunchSetting);
            args = this.ArgumentsBuilder.Build();

            var natives = new DirectoryInfo(this.LaunchSetting.NativesFolder != null && this.LaunchSetting.NativesFolder.Exists
                ? this.LaunchSetting.NativesFolder.FullName.ToString()
                : Path.Combine(core.Root.FullName, "versions", core.Id, "natives"));

            NativesDecompressor.Decompress(natives, core.LibraryResources);

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = this.LaunchSetting.JvmSetting.Javaw.FullName,
                    Arguments = string.Join(' '.ToString(), args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = this.LaunchSetting.EnableIndependencyCore && (bool)this.LaunchSetting.WorkingFolder?.Exists
                        ? this.LaunchSetting.WorkingFolder.FullName
                        : core.Root.FullName
                },
                EnableRaisingEvents = true
            };

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            return new LaunchResponse(process, LaunchState.Succeess, args)
            {
                RunTime = stopWatch,
                EnableXmlFormat = (bool)(this.LaunchSetting.XmlOutputSetting?.Enable)
            };
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(OperationCanceledException))
                return new LaunchResponse(process, LaunchState.Cancelled, args);

            return new LaunchResponse(process, LaunchState.Failed, args, ex);
        }
    }

    public LaunchResponse LaunchMinecraft(string id, Action<LaunchProgressChangedEventArgs> action)
        => this.LaunchMinecraftAsync(id, action).GetAwaiter().GetResult();

    public async Task<LaunchResponse> LaunchMinecraftAsync(string id, Action<LaunchProgressChangedEventArgs> action)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        IProgress<LaunchProgressChangedEventArgs> progress = new Progress<LaunchProgressChangedEventArgs>();
        ((Progress<LaunchProgressChangedEventArgs>)progress).ProgressChanged += MinecraftLauncher_ProgressChanged;

        void MinecraftLauncher_ProgressChanged(object _, LaunchProgressChangedEventArgs e)
        {
            action(e);

            if (e.CancellationToken.IsCancellationRequested)
                e.CancellationToken.ThrowIfCancellationRequested();
        }

        IEnumerable<string> args = new string[] { };
        Process process = null;

        try
        {
            var core = this.GameCoreLocator.GetGameCore(id);
            progress.Report(LaunchProgressChangedEventArgs.Create(0.2f, "正在查找游戏核心", cancellationTokenSource.Token));

            if (core == null)
            {
                ((Progress<LaunchProgressChangedEventArgs>)progress).ProgressChanged -= MinecraftLauncher_ProgressChanged;
                return new LaunchResponse(null, LaunchState.Failed, null);
            }

            if (this.ResourceDownloader != null)
            {
                this.ResourceDownloader.GameCore = core;
                progress.Report(LaunchProgressChangedEventArgs.Create(0.4f, "正在补全游戏文件", cancellationTokenSource.Token));
                await this.ResourceDownloader.DownloadAsync();
            }

            progress.Report(LaunchProgressChangedEventArgs.Create(0.6f, "正在验证账户信息", cancellationTokenSource.Token));
            if (this.Authenticator != null)
                this.LaunchSetting.Account = await this.Authenticator.AuthenticateAsync();

            progress.Report(LaunchProgressChangedEventArgs.Create(0.8f, "正在构建启动参数", cancellationTokenSource.Token));
            this.ArgumentsBuilder = new ArgumentsBuilder(core, this.LaunchSetting);
            args = this.ArgumentsBuilder.Build();

            var natives = new DirectoryInfo(this.LaunchSetting.NativesFolder != null && this.LaunchSetting.NativesFolder.Exists
                ? this.LaunchSetting.NativesFolder.FullName.ToString()
                : Path.Combine(core.Root.FullName, "versions", core.Id, "natives"));

            NativesDecompressor.Decompress(natives, core.LibraryResources);

            progress.Report(LaunchProgressChangedEventArgs.Create(1.0f, "正在启动游戏", cancellationTokenSource.Token));

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = this.LaunchSetting.JvmSetting.Javaw.FullName,
                    Arguments = string.Join(' '.ToString(), args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = this.LaunchSetting.EnableIndependencyCore && (bool)this.LaunchSetting.WorkingFolder?.Exists
                        ? this.LaunchSetting.WorkingFolder.FullName
                        : core.Root.FullName
                },
                EnableRaisingEvents = true
            };

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            ((Progress<LaunchProgressChangedEventArgs>)progress).ProgressChanged -= MinecraftLauncher_ProgressChanged;
            return new LaunchResponse(process, LaunchState.Succeess, args)
            {
                RunTime = stopWatch,
                EnableXmlFormat = (bool)(this.LaunchSetting.XmlOutputSetting?.Enable)
            };
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(OperationCanceledException))
            {
                ((Progress<LaunchProgressChangedEventArgs>)progress).ProgressChanged -= MinecraftLauncher_ProgressChanged;
                return new LaunchResponse(process, LaunchState.Cancelled, args);
            }

            ((Progress<LaunchProgressChangedEventArgs>)progress).ProgressChanged -= MinecraftLauncher_ProgressChanged;
            return new LaunchResponse(process, LaunchState.Failed, args, ex);
        }
    }
}

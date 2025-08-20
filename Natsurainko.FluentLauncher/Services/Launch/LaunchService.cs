using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Natsurainko.FluentLauncher.Exceptions;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using Nrk.FluentCore.Exceptions;
using Nrk.FluentCore.Experimental.Launch;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Dependencies;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Win32;

namespace Natsurainko.FluentLauncher.Services.Launch;

using static Natsurainko.FluentLauncher.Services.Launch.LaunchProgress;
using PreCheckData = (
    string Java,
    string GameDirectory,
    int MaxMemory,
    int MinMemory,
    Account Account,
    string[] ExtraVmParameters,
    string[] ExtraGameParameters,
    string? GameWindowTitle);

internal class LaunchService(
    SettingsService settingsService,
    AccountService accountService,
    DownloadService downloadService,
    HttpClient httpClient,
    ILogger<LaunchService> logger)
{
    public ObservableCollection<LaunchTaskViewModel> LaunchTasks { get; } = [];

    public int RunningTasks => LaunchTasks.Count(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared);

    public void LaunchFromUI(MinecraftInstance instance)
    {   
        LaunchTaskViewModel launchTask = new(instance, this);
        LaunchTasks.Insert(0, launchTask);
        launchTask.EnqueueAsync().Forget();

        WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("Tasks/Launch"));
    }

    public void LaunchFromUIWithTrack(MinecraftInstance instance)
    {
        LaunchTaskViewModel launchTask = new(instance, this);
        launchTask.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "TaskState")
            {
                if (launchTask.TaskState != TaskState.Prepared && launchTask.TaskState != TaskState.Running)
                    WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(null)); // Cancel the tracking task
            }
            else if (e.PropertyName == "WaitedForInputIdle")
            {
                if (launchTask.WaitedForInputIdle)
                    WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(null)); // Cancel the tracking task
            }
        };

        LaunchTasks.Insert(0, launchTask);
        launchTask.EnqueueAsync().Forget();

        WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(launchTask));
    }

    public async Task<MinecraftProcess> LaunchAsync(
        MinecraftInstance instance,
        DataReceivedEventHandler? outputDataReceivedHandler = null,
        DataReceivedEventHandler? errorDataReceivedHandler = null,
        IProgress<LaunchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        MinecraftProcess? minecraftProcess = null;
        LaunchStage stage = LaunchStage.CheckNeeds;
        logger.LaunchingMinecraftInstance(instance.InstanceId, instance.GetDisplayName());

        try
        {
            InstanceConfig config = instance.GetConfig();
            config.LastLaunchTime = DateTime.Now;
            await App.GetService<QuickLaunchService>().AddLatestMinecraftInstance(instance);

            var preCheckData = await PreCheckLaunchNeeds(instance, config, cancellationToken, progress);

            stage = LaunchStage.Authenticate;
            preCheckData = await RefreshAccount(preCheckData, cancellationToken, progress);

            stage = LaunchStage.CompleteResources;
            await ResolveDependencies(instance, cancellationToken, progress);

            stage = LaunchStage.BuildArguments;
            minecraftProcess = BuildProcess(instance, preCheckData, cancellationToken, progress);

            stage = LaunchStage.CompleteResources;
            LaunchProcess(minecraftProcess, cancellationToken, progress, outputDataReceivedHandler, errorDataReceivedHandler);
        }
        catch (Exception ex)
        {
            minecraftProcess?.Dispose();

            progress?.Report(new(stage, LaunchStageProgress.Failed()));
            logger.FailedToLaunchMinecraftInstance(ex, instance.InstanceId, instance.GetDisplayName());

            throw;
        }

        logger.MinecraftProcessCreated(instance.InstanceId, minecraftProcess.JavaPath);
        return minecraftProcess;
    }

    async Task<PreCheckData> PreCheckLaunchNeeds(
        MinecraftInstance instance,
        InstanceConfig instanceConfig,
        CancellationToken cancellationToken,
        IProgress<LaunchProgress>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new(
            LaunchStage.CheckNeeds,
            LaunchStageProgress.Starting()
        ));

        PreCheckData preCheckData = new();

        #region GameDirectory

        cancellationToken.ThrowIfCancellationRequested();
        preCheckData.GameDirectory = instance.GetGameDirectory();

        if (!Directory.Exists(preCheckData.GameDirectory))
            throw new InstanceDirectoryNotFoundException(instance, preCheckData.GameDirectory);

        #endregion

        #region Java

        cancellationToken.ThrowIfCancellationRequested();

        if (settingsService.Javas.Count == 0 || 
            (!settingsService.EnableAutoJava && string.IsNullOrEmpty(settingsService.ActiveJava)))
            throw new NoActiveJavaRuntimeException();

        if (settingsService.EnableAutoJava)
        {
            var targetJavaVersion = instance.GetSuitableJavaVersion();
            var javaInfos = settingsService.Javas
                .Where(f => FileInfoExtensions.TryParse(f, out var fileInfo) && fileInfo.Exists && 
                    (fileInfo.LinkTarget is null || File.Exists(fileInfo.LinkTarget)))
                .Select(JavaUtils.GetJavaInfo).ToArray();

            JavaInfo[] possiblyAvailableJavas;
            bool isForgeOrNeoForge = false;

            if (instance is ModifiedMinecraftInstance modifiedMinecraftInstance)
            {
                foreach (var loader in modifiedMinecraftInstance.ModLoaders)
                {
                    if (loader.Type == ModLoaderType.NeoForge || loader.Type == ModLoaderType.Forge)
                    {
                        isForgeOrNeoForge = true;
                        break;
                    }
                }
            }

            possiblyAvailableJavas = isForgeOrNeoForge
                ? [.. javaInfos.Where(x => x.Version.Major == targetJavaVersion)]
                : [.. javaInfos.Where(x => x.Version.Major >= targetJavaVersion)];

            if (possiblyAvailableJavas.Length == 0)
                throw new JavaRuntimeIncompatibleException(targetJavaVersion);

            preCheckData.Java = possiblyAvailableJavas.MaxBy(x => x.Version)!.FilePath;
        }
        else
        {
            preCheckData.Java = settingsService.ActiveJava;

            if (!FileInfoExtensions.TryParse(preCheckData.Java, out var fileInfo) || !fileInfo.Exists ||
                (fileInfo.LinkTarget is not null && !File.Exists(fileInfo.LinkTarget)))
                throw new JavaRuntimeFileNotFoundException(preCheckData.Java);
        }

        #endregion

        #region JavaMemory

        cancellationToken.ThrowIfCancellationRequested();

        var javaInfo = JavaUtils.GetJavaInfo(preCheckData.Java);

        var (maxMemory, minMemory) = settingsService.EnableAutoMemory
            ? MemoryUtils.CalculateJavaMemory()
            : (settingsService.JavaMemory, settingsService.JavaMemory);

        // QUESTION: >= 512 or > 1024?
        // 经过实际测试经过实际测试，在使用 x86 的 Java 时候，
        // 设置 1024MB（甚至更小）内存时依旧会报错，在设置 512 MB 的才能启动，
        // 具体阈值在大约多少内存我还未进行测量，所以我在内存设置大于 512 MB 时就阻止启动
        // （一些其他的启动器现在已经禁止了使用 x86 Java 启动游戏）
        if (javaInfo.Architecture != "x64" && maxMemory >= 512)
            throw new X86JavaRuntimeMemoryException();

        preCheckData.MaxMemory = maxMemory;
        preCheckData.MinMemory = minMemory;

        #endregion

        #region Account

        cancellationToken.ThrowIfCancellationRequested();

        if (instanceConfig.EnableSpecialSetting && instanceConfig.EnableTargetedAccount && instanceConfig.Account != null)
        {
            preCheckData.Account = accountService.Accounts.FirstOrDefault(x => x.ProfileEquals(instanceConfig.Account))
                ?? throw new AccountNotFoundException(instanceConfig.Account);
        }
        else preCheckData.Account = accountService.ActiveAccount ?? throw new NoActiveAccountException();

        #endregion

        #region ExtraGameParameters

        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<string> GetExtraGameParameters()
        {
            if (instanceConfig.EnableSpecialSetting)
            {
                if (instanceConfig.EnableFullScreen) yield return "--fullscreen";
                if (instanceConfig.GameWindowWidth > 0) yield return $"--width {instanceConfig.GameWindowWidth}";
                if (instanceConfig.GameWindowHeight > 0) yield return $"--height {instanceConfig.GameWindowHeight}";

                if (!string.IsNullOrEmpty(instanceConfig.ServerAddress))
                {
                    instanceConfig.ServerAddress.ParseServerAddress(out var host, out var port);

                    yield return $"--server {host}";
                    yield return $"--port {port}";
                }
            }
            else
            {
                if (settingsService.EnableFullScreen) yield return "--fullscreen";
                if (settingsService.GameWindowWidth > 0) yield return $"--width {settingsService.GameWindowWidth}";
                if (settingsService.GameWindowHeight > 0) yield return $"--height {settingsService.GameWindowHeight}";

                if (!string.IsNullOrEmpty(settingsService.GameServerAddress))
                {
                    settingsService.GameServerAddress.ParseServerAddress(out var host, out var port);

                    yield return $"--server {host}";
                    yield return $"--port {port}";
                }
            }
        }
        preCheckData.ExtraGameParameters = GetExtraGameParameters().ToArray();

        #endregion

        #region ExtraVmParameters

        cancellationToken.ThrowIfCancellationRequested();

        async IAsyncEnumerable<string> GetExtraVmParameters([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return $"-Dfile.encoding=UTF-8";
            yield return $"-Dstdout.encoding=UTF-8";
            yield return $"-Dstderr.encoding=UTF-8";

            // 修复部分 Log4j 版本不接受活动代码页 65001
            // 原来引发的异常 java.nio.charset.unsupportedcharsetexception: cp65001
            yield return $"-Dsun.stdout.encoding=UTF-8";
            yield return $"-Dsun.err.encoding=UTF-8";

            if (preCheckData.Account is YggdrasilAccount yggdrasil)
            {
                var content = await httpClient.GetStringAsync(yggdrasil.YggdrasilServerUrl, cancellationToken);

                yield return $"-javaagent:{Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "Libs", "authlib-injector-1.2.5.jar").ToPathParameter()}={yggdrasil.YggdrasilServerUrl}";
                yield return "-Dauthlibinjector.side=client";
                yield return $"-Dauthlibinjector.yggdrasil.prefetched={content.ConvertToBase64()}";
            }

            if (instanceConfig.EnableSpecialSetting && instanceConfig.VmParameters != null)
            {
                foreach (var item in instanceConfig.VmParameters)
                    yield return item;
            }
        }

        var extraVmParameters = new List<string>();

        await foreach (var args in GetExtraVmParameters(cancellationToken))
            extraVmParameters.Add(args);
        preCheckData.ExtraVmParameters = [.. extraVmParameters];

        #endregion

        #region GameWindowTitle

        cancellationToken.ThrowIfCancellationRequested();

        if (instanceConfig.EnableSpecialSetting)
        {
            preCheckData.GameWindowTitle = !string.IsNullOrEmpty(instanceConfig.GameWindowTitle)
                ? instanceConfig.GameWindowTitle
                : null;
        }
        else
        {
            preCheckData.GameWindowTitle = !string.IsNullOrEmpty(settingsService.GameWindowTitle)
                ? settingsService.GameWindowTitle
                : null;
        }

        #endregion

        progress?.Report(new(
            LaunchStage.CheckNeeds,
            LaunchStageProgress.Finished()
        ));

        return preCheckData;
    }

    async Task<PreCheckData> RefreshAccount(
        PreCheckData preCheckData,
        CancellationToken cancellationToken,
        IProgress<LaunchProgress>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new(
            LaunchStage.Authenticate,
            LaunchStageProgress.Starting()
        ));

        preCheckData.Account = await accountService.RefreshAccountAsync(preCheckData.Account, cancellationToken);

        progress?.Report(new(
            LaunchStage.Authenticate,
            LaunchStageProgress.Finished()
        ));

        return preCheckData;
    }

    async Task ResolveDependencies(
        MinecraftInstance instance, 
        CancellationToken cancellationToken,
        IProgress<LaunchProgress>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new(
            LaunchStage.CompleteResources,
            LaunchStageProgress.Starting()
        ));

        var dependencyResolver = new DependencyResolver(instance)
        {
            DefalutPreferredVerificationMethod = (DependencyResolver.PreferredVerificationMethod)settingsService.GameFilePreferredVerificationMethod
        };

        dependencyResolver.InvalidDependenciesDetermined += (_, e)
            => progress?.Report(new(
                LaunchStage.CompleteResources,
                LaunchStageProgress.UpdateTotalTasks(e.Count())
            ));
        dependencyResolver.DependencyDownloaded += (_, _)
            => progress?.Report(new(
                LaunchStage.CompleteResources,
                LaunchStageProgress.IncrementFinishedTasks()
            ));

        var groupDownloadResult = await dependencyResolver.VerifyAndDownloadDependenciesAsync(downloadService.Downloader, 10, cancellationToken);

        if (groupDownloadResult.Failed.Count > 0)
            throw new IncompleteDependenciesException(groupDownloadResult.Failed, "Some dependent files encountered errors during download");

        // TODO: 考虑集成到DependencyResolver?
        // Natives decompression
        if (!CanSkipNativesDecompression(instance))
        {
            var (_, nativeLibs) = instance is ModifiedMinecraftInstance { HasInheritance: true } modifiedMinecraftInstance
                ? modifiedMinecraftInstance.InheritedMinecraftInstance.GetRequiredLibraries()
                : instance.GetRequiredLibraries();

            UnzipUtils.BatchUnzip(
                Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId, "natives"),
                nativeLibs.Select(x => x.FullPath));
        }

        progress?.Report(new(
            LaunchStage.CompleteResources,
            LaunchStageProgress.Finished()
        ));
    }

    MinecraftProcess BuildProcess(
        MinecraftInstance instance,
        PreCheckData preCheckData, 
        CancellationToken cancellationToken,
        IProgress<LaunchProgress>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new(
            LaunchStage.BuildArguments,
            LaunchStageProgress.Starting()
        ));

        MinecraftProcess mcProcess = new MinecraftProcessBuilder(instance)
            .SetGameDirectory(preCheckData.GameDirectory)
            .SetJavaSettings(preCheckData.Java, preCheckData.MaxMemory, preCheckData.MinMemory)
            .SetAccountSettings(preCheckData.Account, settingsService.EnableDemoUser)
            .AddVmArguments(preCheckData.ExtraVmParameters)
            .AddGameArguments(preCheckData.ExtraGameParameters)
            .Build();

        mcProcess.Started += (_, _) =>
        {
            if (string.IsNullOrEmpty(preCheckData.GameWindowTitle))
                return;

            Task.Run(async () =>
            {
                try
                {
                    while (mcProcess.State == MinecraftProcessState.Running)
                    {
                        PInvoke.SetWindowText((Windows.Win32.Foundation.HWND)mcProcess.MainWindowHandle, preCheckData.GameWindowTitle);
                        await Task.Delay(1000);
                    }
                }
                finally { }
            });
        };

        progress?.Report(new(
            LaunchStage.BuildArguments,
            LaunchStageProgress.Finished()
        ));

        return mcProcess;
    }

    static void LaunchProcess(
        MinecraftProcess mcProcess,
        CancellationToken cancellationToken,
        IProgress<LaunchProgress>? progress,
        DataReceivedEventHandler? outputDataReceivedHandler = null,
        DataReceivedEventHandler? errorDataReceivedHandler = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new(
            LaunchStage.LaunchProcess,
            LaunchStageProgress.Starting()
        ));

        if (outputDataReceivedHandler != null)
            mcProcess.OutputDataReceived += outputDataReceivedHandler;
        if (errorDataReceivedHandler != null)
            mcProcess.ErrorDataReceived += errorDataReceivedHandler;

        mcProcess.Start();

        progress?.Report(new(
            LaunchStage.LaunchProcess,
            LaunchStageProgress.Finished()
        ));
    }

    private static bool CanSkipNativesDecompression(MinecraftInstance instance)
    {
        var nativesDirectory = new DirectoryInfo(Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId, "natives"));

        if (!nativesDirectory.Exists) return false;

        var files = nativesDirectory.GetFiles();
        if (files.Length == 0) return false;

        try
        {
            var processes = FileLockUtils.GetProcessesLockingFile(files.First().FullName);

            foreach (var process in processes)
            {
                var arguments = process?.GetCommandLine();

                if ((arguments?.Contains($"-Djava.library.path={nativesDirectory.FullName}")).GetValueOrDefault())
                {
                    processes.ForEach(p => p?.Dispose());
                    return true;
                }

                process?.Dispose();
            }
        }
        catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
        {
            // Intentionally empty - no security access to the process.
        }
        catch (InvalidOperationException)
        {
            // Intentionally empty - the process exited before getting details.
        }
        catch (Exception) { /*  */ }

        return false;
    }
}

internal static partial class LaunchServiceLoggers
{
    [LoggerMessage(LogLevel.Information, "Launching Minecraft instance {InstanceId} ({InstanceName})")]
    public static partial void LaunchingMinecraftInstance(this ILogger logger, string instanceId, string instanceName);

    [LoggerMessage(LogLevel.Information, "Minecraft {instanceId} Process ({executableFile}) created")]
    public static partial void MinecraftProcessCreated(this ILogger logger, string instanceId, string executableFile);

    [LoggerMessage(LogLevel.Error, "Failed to launch Minecraft instance {InstanceId} ({InstanceName})")]
    public static partial void FailedToLaunchMinecraftInstance(this ILogger logger, Exception exception, string instanceId, string instanceName);
}

record struct LaunchProgress(
    LaunchStage Stage,
    LaunchStageProgress StageProgress)
{
    public enum LaunchStage
    {
        CheckNeeds,
        Authenticate,
        CompleteResources,
        BuildArguments,
        LaunchProcess
    }
};
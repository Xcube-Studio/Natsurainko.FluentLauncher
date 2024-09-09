using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using Nrk.FluentCore.Exceptions;
using Nrk.FluentCore.Experimental.Launch;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Dependencies;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Utils;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

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

internal class LaunchService
{
    private readonly DownloadService _downloadService;
    private readonly AccountService _accountService;
    private readonly SettingsService _settingsService;

    public ObservableCollection<LaunchTaskViewModel> LaunchTasks { get; } = [];

    public LaunchService(
        SettingsService settingsService,
        AccountService accountService,
        DownloadService downloadService)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        _downloadService = downloadService;
    }

    public void LaunchFromUI(MinecraftInstance instance)
    {
        var viewModel = new LaunchTaskViewModel(instance);
        App.DispatcherQueue.TryEnqueue(() => LaunchTasks.Insert(0, viewModel));

        viewModel.Start();
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

        try
        {
            InstanceConfig config = instance.GetConfig();
            config.LastLaunchTime = DateTime.Now;
            //App.DispatcherQueue.TryEnqueue(() => config.LastLaunchTime = DateTime.Now);

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
        catch (Exception)
        {
            minecraftProcess?.Dispose();

            progress?.Report(new(stage, LaunchStageProgress.Failed()));
            throw;
        }

        return minecraftProcess;
    }

    async Task<PreCheckData> PreCheckLaunchNeeds(
        MinecraftInstance instance,
        InstanceConfig specialConfig,
        CancellationToken cancellationToken,
        IProgress<LaunchProgress>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new(
            LaunchStage.CheckNeeds,
            LaunchStageProgress.Starting()
        ));

        var preCheckData = new PreCheckData();

        #region GameDirectory

        cancellationToken.ThrowIfCancellationRequested();

        if (specialConfig.EnableSpecialSetting)
        {
            preCheckData.GameDirectory = specialConfig.EnableIndependencyCore
                ? Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId)
                : instance.MinecraftFolderPath;
        }
        else
        {
            preCheckData.GameDirectory = _settingsService.EnableIndependencyCore
                ? Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId)
                : instance.MinecraftFolderPath;
        }

        if (!PathUtils.IsValidPath(preCheckData.GameDirectory) || !Directory.Exists(preCheckData.GameDirectory))
            throw new Exception($"This is not a valid path, or the path does not exist: ({nameof(preCheckData.GameDirectory)}): {preCheckData.GameDirectory}");

        #endregion

        #region Java

        cancellationToken.ThrowIfCancellationRequested();

        if (_settingsService.Javas.Count == 0)
            throw new Exception("No Java added to the launch settings");
        if (!_settingsService.EnableAutoJava && string.IsNullOrEmpty(_settingsService.ActiveJava))
            throw new Exception("Automatic Java selection is not enabled, but no Java is manually enabled.");

        if (_settingsService.EnableAutoJava)
        {
            var targetJavaVersion = instance.GetSuitableJavaVersion();

            var javaInfos = _settingsService.Javas.Select(JavaUtils.GetJavaInfo).ToArray();
            var possiblyAvailableJavas = targetJavaVersion == null
                ? javaInfos
                : javaInfos.Where(x => x.Version.Major.ToString().Equals(targetJavaVersion)).ToArray();

            if (possiblyAvailableJavas.Length == 0)
                throw new Exception($"No suitable version of Java found to start this game, version {targetJavaVersion} is required");

            preCheckData.Java = possiblyAvailableJavas.MaxBy(x => x.Version)!.FilePath;
        }
        else preCheckData.Java = _settingsService.ActiveJava;

        if (!PathUtils.IsValidPath(preCheckData.Java) || !File.Exists(preCheckData.Java))
            throw new Exception($"This is not a valid path, or the path does not exist: ({nameof(preCheckData.Java)}): {preCheckData.Java}");

        #endregion

        #region JavaMemory

        cancellationToken.ThrowIfCancellationRequested();

        var javaInfo = JavaUtils.GetJavaInfo(preCheckData.Java);

        var (maxMemory, minMemory) = _settingsService.EnableAutoMemory
            ? MemoryUtils.CalculateJavaMemory()
            : (_settingsService.JavaMemory, _settingsService.JavaMemory);

        // QUESTION: >= 512 or > 1024?
        // 经过实际测试经过实际测试，在使用 x86 的 Java 时候，
        // 设置 1024MB（甚至更小）内存时依旧会报错，在设置 512 MB 的才能启动，
        // 具体阈值在大约多少内存我还未进行测量，所以我在内存设置大于 512 MB 时就阻止启动
        // （一些其他的启动器现在已经禁止了使用 x86 Java 启动游戏）
        if (javaInfo.Architecture != "x64" && maxMemory >= 512)
            throw new Exception("Using a 32-bit Java, but have allocated more than 1(or 0.5) GB of memory. " +
                "Please use a 64-bit Java, or turn off automatic memory allocation and manually allocate less than 1(or 0.5) GB of memory.");

        preCheckData.MaxMemory = maxMemory;
        preCheckData.MinMemory = minMemory;

        #endregion

        #region Account

        cancellationToken.ThrowIfCancellationRequested();

        if (specialConfig.EnableSpecialSetting && specialConfig.EnableTargetedAccount && specialConfig.Account != null)
        {
            preCheckData.Account = _accountService.Accounts.First(x => x.Equals(specialConfig.Account))
                ?? throw new Exception("The game specifies an account to launch, but that account cannot be found in the current account list");
        }
        else preCheckData.Account = _accountService.ActiveAccount
                ?? throw new Exception("No account selected");

        #endregion

        #region ExtraGameParameters

        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<string> GetExtraGameParameters()
        {
            if (specialConfig.EnableSpecialSetting)
            {
                if (specialConfig.EnableFullScreen) yield return "--fullscreen";
                if (specialConfig.GameWindowWidth > 0) yield return $"--width {specialConfig.GameWindowWidth}";
                if (specialConfig.GameWindowHeight > 0) yield return $"--height {specialConfig.GameWindowHeight}";

                if (!string.IsNullOrEmpty(specialConfig.ServerAddress))
                {
                    specialConfig.ServerAddress.ParseServerAddress(out var host, out var port);

                    yield return $"--server {host}";
                    yield return $"--port {port}";
                }
            }
            else
            {
                if (_settingsService.EnableFullScreen) yield return "--fullscreen";
                if (_settingsService.GameWindowWidth > 0) yield return $"--width {_settingsService.GameWindowWidth}";
                if (_settingsService.GameWindowHeight > 0) yield return $"--height {_settingsService.GameWindowHeight}";

                if (!string.IsNullOrEmpty(_settingsService.GameServerAddress))
                {
                    specialConfig.ServerAddress.ParseServerAddress(out var host, out var port);

                    yield return $"--server {host}";
                    yield return $"--port {port}";
                }
            }
        }
        preCheckData.ExtraGameParameters = GetExtraGameParameters().ToArray();

        #endregion

        #region ExtraVmParameters

        cancellationToken.ThrowIfCancellationRequested();

        async IAsyncEnumerable<string> GetExtraVmParameters(CancellationToken cancellationToken)
        {
            if (preCheckData.Account is YggdrasilAccount yggdrasil)
            {
                var content = await HttpUtils.HttpClient.GetStringAsync(yggdrasil.YggdrasilServerUrl, cancellationToken);

                yield return $"-javaagent:{Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "Libs", "authlib-injector-1.2.5.jar").ToPathParameter()}={yggdrasil.YggdrasilServerUrl}";
                yield return "-Dauthlibinjector.side=client";
                yield return $"-Dauthlibinjector.yggdrasil.prefetched={content.ConvertToBase64()}";
            }

            if (specialConfig.EnableSpecialSetting && specialConfig.VmParameters != null)
            {
                foreach (var item in specialConfig.VmParameters)
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

        if (specialConfig.EnableSpecialSetting)
        {
            preCheckData.GameWindowTitle = !string.IsNullOrEmpty(specialConfig.GameWindowTitle)
                ? specialConfig.GameWindowTitle
                : null;
        }
        else
        {
            preCheckData.GameWindowTitle = !string.IsNullOrEmpty(_settingsService.GameWindowTitle)
                ? _settingsService.GameWindowTitle
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

        preCheckData.Account = await _accountService.RefreshAccountAsync(preCheckData.Account);

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

        var dependencyResolver = new DependencyResolver(instance);

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

        var groupDownloadResult = await dependencyResolver.VerifyAndDownloadDependenciesAsync(_downloadService.Downloader, 10, cancellationToken);

        if (groupDownloadResult.Failed.Count > 0)
            throw new IncompleteDependenciesException(groupDownloadResult.Failed, "Some dependent files encountered errors during download");

        // TODO: 考虑集成到DependencyResolver?
        // Natives decompression
        if (!CanSkipNativesDecompression(instance))
        {
            var (_, nativeLibs) = instance.GetRequiredLibraries();
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
            .SetAccountSettings(preCheckData.Account, _settingsService.EnableDemoUser)
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
                        User32.SetWindowText(mcProcess.MainWindowHandle, preCheckData.GameWindowTitle);
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


/*
public record struct LaunchProgress(
    LaunchSessionState State,
    DependencyResolver? DependencyResolver,
    MinecraftProcess? MinecraftProcess,
    Exception? Exception);

public enum LaunchSessionState
{
    // Launch sequence
    Created = 0,
    Inspecting = 1,
    Authenticating = 2,
    CompletingResources = 3,
    BuildingArguments = 4,
    LaunchingProcess = 5,
    GameRunning = 6,

    GameExited = 7, // Game exited normally (exit code == 0)
    Faulted = 8, // Failure before game started
    Killed = 9, // Game killed by user
    GameCrashed = 10 // Game crashed (exit code != 0)
}*/
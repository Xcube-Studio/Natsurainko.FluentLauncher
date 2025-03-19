using CommunityToolkit.Mvvm.Messaging;
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
    HttpClient httpClient)
{
    public event EventHandler? TaskListStateChanged;

    public ObservableCollection<LaunchTaskViewModel> LaunchTasks { get; } = [];

    public void LaunchFromUI(MinecraftInstance instance)
    {
        var viewModel = new LaunchTaskViewModel(instance, this);
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "TaskState")
                TaskListStateChanged?.Invoke(this, e);
        };

        InsertTask(viewModel);
        viewModel.Start();

        WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("Tasks/Launch"));
    }

    public void LaunchFromUIWithTrack(MinecraftInstance instance)
    {
        var viewModel = new LaunchTaskViewModel(instance, this);
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "TaskState")
            {
                TaskListStateChanged?.Invoke(this, e);

                if (viewModel.TaskState != TaskState.Prepared && viewModel.TaskState != TaskState.Running)
                    WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(null)); // Cancel the tracking task
            }
            else if (e.PropertyName == "WaitedForInputIdle")
            {
                if (viewModel.WaitedForInputIdle)
                    WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(null)); // Cancel the tracking task
            }
        };

        InsertTask(viewModel);
        viewModel.Start();

        WeakReferenceMessenger.Default.Send(new TrackLaunchTaskChangedMessage(viewModel));
    }

    private void InsertTask(LaunchTaskViewModel task)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            LaunchTasks.Insert(0, task);
            TaskListStateChanged?.Invoke(this, EventArgs.Empty);
        });
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
            preCheckData.GameDirectory = settingsService.EnableIndependencyCore
                ? Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId)
                : instance.MinecraftFolderPath;
        }

        if (!PathUtils.IsValidPath(preCheckData.GameDirectory) || !Directory.Exists(preCheckData.GameDirectory))
            throw new Exception($"This is not a valid path, or the path does not exist: ({nameof(preCheckData.GameDirectory)}): {preCheckData.GameDirectory}");

        #endregion

        #region Java

        cancellationToken.ThrowIfCancellationRequested();

        if (settingsService.Javas.Count == 0)
            throw new Exception("No Java added to the launch settings");
        if (!settingsService.EnableAutoJava && string.IsNullOrEmpty(settingsService.ActiveJava))
            throw new Exception("Automatic Java selection is not enabled, but no Java is manually enabled.");

        if (settingsService.EnableAutoJava)
        {
            var targetJavaVersion = instance.GetSuitableJavaVersion();
            var javaInfos = settingsService.Javas.Select(JavaUtils.GetJavaInfo).ToArray();

            JavaInfo[] possiblyAvailableJavas;
            bool isForgeOrNeoForge = false;

            if (instance is ModifiedMinecraftInstance modifiedMinecraftInstance)
            {
                var loaders = modifiedMinecraftInstance.ModLoaders.Select(x => x.Type);
                isForgeOrNeoForge = loaders.Contains(ModLoaderType.Forge) || loaders.Contains(ModLoaderType.NeoForge);
            }

            possiblyAvailableJavas = targetJavaVersion == null
                ? javaInfos
                : isForgeOrNeoForge
                    ? javaInfos.Where(x => x.Version.Major.ToString().Equals(targetJavaVersion)).ToArray()
                    : javaInfos.Where(x => x.Version.Major >= int.Parse(targetJavaVersion)).ToArray();

            if (possiblyAvailableJavas.Length == 0)
                throw new Exception($"No suitable version of Java found to start this game, version {targetJavaVersion} is required");

            preCheckData.Java = possiblyAvailableJavas.MaxBy(x => x.Version)!.FilePath;
        }
        else preCheckData.Java = settingsService.ActiveJava;

        if (!PathUtils.IsValidPath(preCheckData.Java, isFile: true) || !File.Exists(preCheckData.Java))
            throw new Exception($"This is not a valid path, or the path does not exist: ({nameof(preCheckData.Java)}): {preCheckData.Java}");

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
            throw new Exception("Using a 32-bit Java, but have allocated more than 1(or 0.5) GB of memory. " +
                "Please use a 64-bit Java, or turn off automatic memory allocation and manually allocate less than 1(or 0.5) GB of memory.");

        preCheckData.MaxMemory = maxMemory;
        preCheckData.MinMemory = minMemory;

        #endregion

        #region Account

        cancellationToken.ThrowIfCancellationRequested();

        if (specialConfig.EnableSpecialSetting && specialConfig.EnableTargetedAccount && specialConfig.Account != null)
        {
            preCheckData.Account = accountService.Accounts.FirstOrDefault(x => x.ProfileEquals(specialConfig.Account))
                ?? throw new Exception("The game specifies an account to launch, but that account cannot be found in the current account list");
        }
        else preCheckData.Account = accountService.ActiveAccount
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
            if (preCheckData.Account is YggdrasilAccount yggdrasil)
            {
                var content = await httpClient.GetStringAsync(yggdrasil.YggdrasilServerUrl, cancellationToken);

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
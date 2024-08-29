using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class LaunchService
{
    private readonly AuthenticationService _authenticationService;
    private readonly DownloadService _downloadService;
    private readonly AccountService _accountService;
    private readonly SettingsService _settingsService;

    public ObservableCollection<LaunchSessionViewModel> LaunchSessions { get; } = new();

    public LaunchService(
        SettingsService settingsService,
        AccountService accountService,
        AuthenticationService authenticationService,
        DownloadService downloadService)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        _accountService = accountService;
        _downloadService = downloadService;
    }

    public async Task LaunchFromUIAsync(MinecraftInstance instance)
    {
        var viewModel = new LaunchSessionViewModel(instance);
        LaunchSessions.Insert(0, viewModel);
        await LaunchAsync(instance, viewModel, viewModel.LaunchCancellationToken);
    }

    public async Task LaunchAsync(
        MinecraftInstance instance,
        IProgress<LaunchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            progress?.Report(new (LaunchSessionState.Inspecting, null, null, null));

            // 1. Update Jumplist
            instance.UpdateLastLaunchTimeToNow();
            //await JumpListService.UpdateJumpListAsync(instance);

            // 2. Check environment
            cancellationToken.ThrowIfCancellationRequested();

            // 2.1. Java path
            string? suitableJava = null;

            if (string.IsNullOrEmpty(_settingsService.ActiveJava))
                throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoActiveJava"));

            suitableJava = _settingsService.EnableAutoJava ? GetSuitableJava(instance) : _settingsService.ActiveJava;
            if (suitableJava == null)
                throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoSuitableJava").Replace("${version}", instance.GetSuitableJavaVersion()));

            // 2.2. Java memory
            var (maxMemory, minMemory) = _settingsService.EnableAutoJava
                ? MemoryUtils.CalculateJavaMemory()
                : (_settingsService.JavaMemory, _settingsService.JavaMemory);

            if (JavaUtils.GetJavaInfo(suitableJava).Architecture != "x64" && maxMemory >= 512) // QUESTION: >= 512 or > 1024?
                throw new Exception(ResourceUtils.GetValue("Exceptions", "_x86_JavaMemoryException"));

            // 3. Get account
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new(LaunchSessionState.Authenticating, null, null, null));

            InstanceConfig config = instance.GetConfig();
            Account? account = GetLaunchAccount(config, _accountService)
                ?? throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount")); // Determine which account to use

            if (account is null)
                throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount"));

            var refreshedAccount = await RefreshAccountAsync(account, config);

            // 4. Resolve dependencies
            cancellationToken.ThrowIfCancellationRequested();
            await ResolveDependenciesAsync(instance, progress, cancellationToken);

            // 5. Start MinecraftProcess
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new(LaunchSessionState.BuildingArguments, null, null, null));

            MinecraftProcess mcProcess = new MinecraftProcessBuilder(instance)
                .SetAccountSettings(refreshedAccount, _settingsService.EnableDemoUser)
                .SetJavaSettings(suitableJava, maxMemory, minMemory)
                .SetGameDirectory(GetGameDirectory(instance, config))
                .AddVmArguments(GetExtraVmParameters(config, account))
                .AddGameArguments(GetExtraGameParameters(config))
                .Build();

            mcProcess.Exited += (_, _) => mcProcess.Dispose();
            mcProcess.Started += (_, _) =>
            {
                var title = GameWindowTitle(config);
                if (string.IsNullOrEmpty(title)) return;

                Task.Run(async () =>
                {
                    try
                    {
                        while (mcProcess.State == MinecraftProcessState.Running)
                        {
                            User32.SetWindowText(mcProcess.MainWindowHandle, title);
                            await Task.Delay(1000);
                        }
                    }
                    catch { }
                });
            };

            progress?.Report(new(LaunchSessionState.LaunchingProcess, null, mcProcess, null));
            mcProcess.Start();

            progress?.Report(new(LaunchSessionState.GameRunning, null, null, null));
        }
        catch (Exception e)
        {
            progress?.Report(new(LaunchSessionState.Faulted, null, null, e));
        }
    }

    private async Task<Account> RefreshAccountAsync(Account account, InstanceConfig config)
    {
        if (account.Equals(_accountService.ActiveAccount))
        {
            await _accountService.RefreshActiveAccount();
            return _accountService.ActiveAccount;
        }
        else
        {
            await _accountService.RefreshAccount(account);
            return GetLaunchAccount(config, _accountService);
        }
    }

    private async Task ResolveDependenciesAsync(MinecraftInstance instance, IProgress<LaunchProgress>? progress, CancellationToken cancellationToken)
    {
        var resolver = new DependencyResolver(instance);
        progress?.Report(new(LaunchSessionState.CompletingResources, resolver, null, null));

        var downloadResult = await resolver.VerifyAndDownloadDependenciesAsync(_downloadService.Downloader, 10, cancellationToken);
        if (downloadResult.Failed.Count > 0)
            throw new IncompleteGameResourcesException(downloadResult.Failed.Select(r => r.Item2));

        // TODO: 考虑集成到DependencyResolver?
        // Natives decompression
        if (!CanSkipNativesDecompression(instance))
        {
            var (_, nativeLibs) = instance.GetRequiredLibraries();
            UnzipUtils.BatchUnzip(
                Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId, "natives"),
                nativeLibs.Select(x => x.FullPath));
        }
    }

    private bool CanSkipNativesDecompression(MinecraftInstance instance)
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

    private string? GetSuitableJava(MinecraftInstance MinecraftInstance)
    {
        var regex = new Regex(@"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()~!@#$%^&()\[\]{}+=]+\\?)*$");

        var javaVersion = MinecraftInstance.GetSuitableJavaVersion();
        var suits = new List<(string, Version)>();

        foreach (var java in _settingsService.Javas)
        {
            if (!regex.IsMatch(java) || !File.Exists(java)) continue;

            var info = JavaUtils.GetJavaInfo(java);
            if (info.Version.Major.ToString().Equals(javaVersion))
            {
                suits.Add((java, info.Version));
                suits.Sort((a, b) => -a.Item2.CompareTo(b.Item2));
            }
        }

        if (!suits.Any())
            return null;

        return suits.First().Item1;
    }

    private string GetGameDirectory(MinecraftInstance instance, InstanceConfig specialConfig)
    {
        if (specialConfig.EnableSpecialSetting)
        {
            if (specialConfig.EnableIndependencyCore)
                return Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId);
            else return instance.MinecraftFolderPath;
        }

        if (_settingsService.EnableIndependencyCore)
            return Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId);

        return instance.MinecraftFolderPath;
    }

    private string? GameWindowTitle(InstanceConfig specialConfig)
    {
        if (specialConfig.EnableSpecialSetting)
        {
            if (!string.IsNullOrEmpty(specialConfig.GameWindowTitle))
                return specialConfig.GameWindowTitle;
        }
        else
        {
            if (!string.IsNullOrEmpty(_settingsService.GameWindowTitle))
                return _settingsService.GameWindowTitle;
        }

        return null;
    }

    public static Account? GetLaunchAccount(InstanceConfig specialConfig, AccountService _accountService)
    {
        if (specialConfig.EnableSpecialSetting && specialConfig.EnableTargetedAccount && specialConfig.Account != null)
        {
            var matchAccount = _accountService.Accounts.Where(account =>
            {
                if (!account.Type.Equals(specialConfig.Account.Type)) return false;
                if (!account.Uuid.Equals(specialConfig.Account.Uuid)) return false;
                if (!account.Name.Equals(specialConfig.Account.Name)) return false;

                if (specialConfig.Account is YggdrasilAccount yggdrasil)
                {
                    if (!((YggdrasilAccount)account).YggdrasilServerUrl.Equals(yggdrasil.YggdrasilServerUrl))
                        return false;
                }

                return true;
            });

            return matchAccount.FirstOrDefault();
        }

        return _accountService.ActiveAccount;
    }

    private IEnumerable<string> GetExtraVmParameters(InstanceConfig specialConfig, Account account)
    {
        if (account is YggdrasilAccount yggdrasil)
        {
            using var res = HttpUtils.HttpGet(yggdrasil.YggdrasilServerUrl);

            yield return $"-javaagent:{Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "Libs", "authlib-injector-1.2.5.jar").ToPathParameter()}={yggdrasil.YggdrasilServerUrl}";
            yield return "-Dauthlibinjector.side=client";
            yield return $"-Dauthlibinjector.yggdrasil.prefetched={(res.Content.ReadAsString()).ConvertToBase64()}";
        }

        if (!specialConfig.EnableSpecialSetting || specialConfig.VmParameters == null)
            yield break;

        foreach (var item in specialConfig.VmParameters)
            yield return item;
    }

    private IEnumerable<string> GetExtraGameParameters(InstanceConfig specialConfig)
    {
        if (specialConfig.EnableSpecialSetting)
        {
            if (specialConfig.EnableFullScreen)
                yield return "--fullscreen";

            if (specialConfig.GameWindowWidth > 0)
                yield return $"--width {specialConfig.GameWindowWidth}";

            if (specialConfig.GameWindowHeight > 0)
                yield return $"--height {specialConfig.GameWindowHeight}";

            if (!string.IsNullOrEmpty(specialConfig.ServerAddress))
            {
                specialConfig.ServerAddress.ParseServerAddress(out var host, out var port);

                yield return $"--server {host}";
                yield return $"--port {port}";
            }
        }
        else
        {
            if (_settingsService.EnableFullScreen)
                yield return "--fullscreen";

            if (_settingsService.GameWindowWidth > 0)
                yield return $"--width {_settingsService.GameWindowWidth}";

            if (_settingsService.GameWindowHeight > 0)
                yield return $"--height {_settingsService.GameWindowHeight}";

            if (!string.IsNullOrEmpty(_settingsService.GameServerAddress))
            {
                specialConfig.ServerAddress.ParseServerAddress(out var host, out var port);

                yield return $"--server {host}";
                yield return $"--port {port}";
            }
        }
    }
}

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
}
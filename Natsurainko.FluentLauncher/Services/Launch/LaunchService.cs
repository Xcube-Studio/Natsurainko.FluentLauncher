using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.SystemServices;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Tasks;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Experimental.GameManagement.Launch;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Utils;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class LaunchService
{
    private readonly AuthenticationService _authenticationService;
    private readonly DownloadService _downloadService;
    private readonly AccountService _accountService;
    private readonly SettingsService _settingsService;
    private readonly LaunchSessions _launchSessions;

    protected readonly List<MinecraftSession> _sessions;
    public ReadOnlyCollection<MinecraftSession> Sessions { get; }

    public event EventHandler<MinecraftSession>? SessionCreated;

    public LaunchService(
        SettingsService settingsService,
        AccountService accountService,
        AuthenticationService authenticationService,
        DownloadService downloadService,
        LaunchSessions launchSessions)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        _accountService = accountService;
        _downloadService = downloadService;
        _launchSessions = launchSessions;

        _sessions = [];
        Sessions = new(_sessions);
    }

    public async Task LaunchGame(MinecraftInstance mcInstance)
    {
        MinecraftSession? minecraftSession = null;
        Action<Exception>? onExceptionThrow = null;

        try
        {
            Account? account = _accountService.ActiveAccount ?? throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount"));

            minecraftSession = CreateMinecraftSessionFromMinecraftInstance(mcInstance, account);
            _sessions.Add(minecraftSession);

            _launchSessions.CreateLaunchSessionViewModel(minecraftSession, out var handleException);
            onExceptionThrow = handleException;

            mcInstance.UpdateLastLaunchTimeToNow();
            await JumpListService.UpdateJumpListAsync(mcInstance);

            await minecraftSession.StartAsync();
        }
        catch (Exception ex)
        {
            (onExceptionThrow ?? OnExceptionThrow).Invoke(ex);
        }
        finally
        {
            if (minecraftSession != null)
            {
                minecraftSession.ProcessExited += (object? sender, MinecraftProcessExitedEventArgs e) =>
                {
                    minecraftSession.McProcess?.Dispose();
                };
            }
        }
    }

    private void OnExceptionThrow(Exception exception)
    {
        string errorDescriptionKey = string.Empty;

        if (exception is InvalidOperationException)
        {

        }
        else if (exception is YggdrasilAuthenticationException)
        {
            errorDescriptionKey = "_LaunchGameThrowYggdrasilAuthenticationException";
        }
        else if (exception is MicrosoftAuthenticationException)
        {
            errorDescriptionKey = "_LaunchGameThrowMicrosoftAuthenticationException";
        }

        App.GetService<NotificationService>().NotifyException(
            "_LaunchGameThrowException",
            exception,
            errorDescriptionKey);
    }

    protected void OnSessionCreated(MinecraftSession minecraftSession)
        => this.SessionCreated?.Invoke(this, minecraftSession);

    public MinecraftSession CreateMinecraftSessionFromMinecraftInstance(MinecraftInstance instance, Account? _)
    {
        Account? account = _accountService.ActiveAccount;
        if (account is null)
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount"));

        // Java
        string? suitableJava = null;

        if (string.IsNullOrEmpty(_settingsService.ActiveJava))
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoActiveJava"));
        // TODO: Do not localize exception message

        suitableJava = _settingsService.EnableAutoJava ? GetSuitableJava(instance) : _settingsService.ActiveJava;
        if (suitableJava == null)
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoSuitableJava").Replace("${version}", instance.GetSuitableJavaVersion()));

        var config = instance.GetConfig(); // Game specific config
        var launchAccount = GetLaunchAccount(config, _accountService)
            ?? throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount")); // Determine which account to use

        Account Authenticate()
        {
            // TODO: refactor to remove dependency on AuthenticationService, and AccountService.
            // Call FluentCore to refresh account directly.
            try
            {
                if (launchAccount.Equals(_accountService.ActiveAccount))
                {
                    _accountService.RefreshActiveAccount().GetAwaiter().GetResult();
                    return _accountService.ActiveAccount;
                }
                else
                {
                    _accountService.RefreshAccount(launchAccount).GetAwaiter().GetResult();
                    return GetLaunchAccount(config, _accountService);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        var (maxMemory, minMemory) = _settingsService.EnableAutoJava
            ? MemoryUtils.CalculateJavaMemory()
            : (_settingsService.JavaMemory, _settingsService.JavaMemory);

        if (JavaUtils.GetJavaInfo(suitableJava).Architecture != "x64" && maxMemory >= 512)
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_x86_JavaMemoryException"));

        var session = new MinecraftSession() // Launch session
        {
            Account = launchAccount,
            MinecraftInstance = instance,
            GameDirectory = GetGameDirectory(instance, config),
            JavaPath = suitableJava,
            MaxMemory = maxMemory,
            MinMemory = minMemory,
            UseDemoUser = _settingsService.EnableDemoUser,
            ExtraGameParameters = GetExtraGameParameters(config),
            ExtraVmParameters = GetExtraVmParameters(config, launchAccount),
            CreateDependencyResolver = (libs) => _downloadService.CreateResourcesDownloader(instance, libs)
        };

        if (_settingsService.AutoRefresh)
            session.RefreshAccountTask = new Task<Account>(Authenticate);

        session.SkipNativesDecompression = CanSkipNativesDecompression(instance);

        session.ProcessStarted += (s, e) =>
        {
            var title = GameWindowTitle(config);
            if (string.IsNullOrEmpty(title)) return;

            Task.Run(async () =>
            {
                try
                {
                    while (session.State == MinecraftSessionState.GameRunning)
                    {
                        User32.SetWindowText(session.GetProcessMainWindowHandle(), title);
                        await Task.Delay(1000);
                    }
                }
                catch { }
            });
        };

        return session;
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

    private string GetGameDirectory(MinecraftInstance instance, GameConfig specialConfig)
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

    private string? GameWindowTitle(GameConfig specialConfig)
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

    public static Account GetLaunchAccount(GameConfig specialConfig, AccountService _accountService)
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

            if (matchAccount.Any())
                return matchAccount.First();
            else throw new Exception("Can't find target account");
        }

        return _accountService.ActiveAccount;
    }

    private IEnumerable<string> GetExtraVmParameters(GameConfig specialConfig, Account account)
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

    private IEnumerable<string> GetExtraGameParameters(GameConfig specialConfig)
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

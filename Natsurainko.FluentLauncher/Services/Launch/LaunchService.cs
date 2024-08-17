using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.SystemServices;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
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
    private readonly NotificationService _notificationService;
    private readonly AccountService _accountService;
    private readonly SettingsService _settingsService;

    private SettingsService AppSettingsService => _settingsService;

    protected readonly List<MinecraftSession> _sessions;
    public ReadOnlyCollection<MinecraftSession> Sessions { get; }

    public event EventHandler<MinecraftSession>? SessionCreated;

    public LaunchService(
        SettingsService settingsService,
        GameService gameService,
        AccountService accountService,
        AuthenticationService authenticationService,
        DownloadService downloadService,
        NotificationService notificationService)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        _accountService = accountService;
        _downloadService = downloadService;
        _notificationService = notificationService;

        _sessions = [];
        Sessions = new(_sessions);
    }

    public async Task LaunchGame(GameInfo gameInfo)
    {
        try
        {
            Account? account = _accountService.ActiveAccount ?? throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount"));

            var session = CreateMinecraftSessionFromGameInfo(gameInfo, account); // TODO: replace with ctor of MinecraftSession
            _sessions.Add(session);

            OnSessionCreated(session);

            gameInfo.UpdateLastLaunchTimeToNow();
            App.GetService<JumpListService>().UpdateJumpList(gameInfo);

            await session.StartAsync();
        }
        catch (Exception ex)
        {
            _notificationService.NotifyWithoutContent(ex.Message);
        }
    }

    protected void OnSessionCreated(MinecraftSession minecraftSession)
        => this.SessionCreated?.Invoke(this, minecraftSession);

    public MinecraftSession CreateMinecraftSessionFromGameInfo(GameInfo gameInfo, Account? _)
    {
        Account? account = _accountService.ActiveAccount;
        if (account is null)
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount"));

        // Java
        string? suitableJava = null;

        if (string.IsNullOrEmpty(_settingsService.ActiveJava))
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoActiveJava"));
        // TODO: Do not localize exception message

        suitableJava = AppSettingsService.EnableAutoJava ? GetSuitableJava(gameInfo) : _settingsService.ActiveJava;
        if (suitableJava == null)
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoSuitableJava").Replace("${version}", gameInfo.GetSuitableJavaVersion()));

        var config = gameInfo.GetConfig(); // Game specific config
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
            catch (Exception ex)
            {
                throw new AuthenticateRefreshAccountException(ex);
            }
        }

        var (maxMemory, minMemory) = AppSettingsService.EnableAutoJava
            ? MemoryUtils.CalculateJavaMemory()
            : (_settingsService.JavaMemory, _settingsService.JavaMemory);

        var session = new MinecraftSession() // Launch session
        {
            Account = launchAccount,
            GameInfo = gameInfo,
            GameDirectory = GetGameDirectory(gameInfo, config),
            JavaPath = suitableJava,
            MaxMemory = maxMemory,
            MinMemory = minMemory,
            UseDemoUser = _settingsService.EnableDemoUser,
            ExtraGameParameters = GetExtraGameParameters(config),
            ExtraVmParameters = GetExtraVmParameters(config, launchAccount),
            CreateResourcesDownloader = (libs) => _downloadService.CreateResourcesDownloader
                (gameInfo, libs)
        };

        if (AppSettingsService.AutoRefresh)
            session.RefreshAccountTask = new Task<Account>(Authenticate);

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

    private string? GetSuitableJava(GameInfo gameInfo)
    {
        var regex = new Regex(@"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()~!@#$%^&()\[\]{}+=]+\\?)*$");

        var javaVersion = gameInfo.GetSuitableJavaVersion();
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

    private string GetGameDirectory(GameInfo gameInfo, GameConfig specialConfig)
    {
        if (specialConfig.EnableSpecialSetting)
        {
            if (specialConfig.EnableIndependencyCore)
                return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);
            else return gameInfo.MinecraftFolderPath;
        }

        if (AppSettingsService.EnableIndependencyCore)
            return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);

        return gameInfo.MinecraftFolderPath;
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
            if (!string.IsNullOrEmpty(AppSettingsService.GameWindowTitle))
                return AppSettingsService.GameWindowTitle;
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
            if (AppSettingsService.EnableFullScreen)
                yield return "--fullscreen";

            if (AppSettingsService.GameWindowWidth > 0)
                yield return $"--width {AppSettingsService.GameWindowWidth}";

            if (AppSettingsService.GameWindowHeight > 0)
                yield return $"--height {AppSettingsService.GameWindowHeight}";

            if (!string.IsNullOrEmpty(AppSettingsService.GameServerAddress))
            {
                specialConfig.ServerAddress.ParseServerAddress(out var host, out var port);

                yield return $"--server {host}";
                yield return $"--port {port}";
            }
        }
    }
}

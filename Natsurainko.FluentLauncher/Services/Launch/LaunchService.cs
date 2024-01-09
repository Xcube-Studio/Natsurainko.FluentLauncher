using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Classes.Exceptions;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Utils;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.StartScreen;

namespace Natsurainko.FluentLauncher.Services.Launch;
#nullable enable

internal class LaunchService
{
    private readonly AuthenticationService _authenticationService;
    private readonly SettingsService _settingsService;
    private readonly AccountService _accountService;
    private readonly GameService _gameService;
    private readonly DownloadService _downloadService;

    private readonly List<MinecraftSession> _sessions = new();

    public ReadOnlyCollection<MinecraftSession> Sessions { get; }

    public event EventHandler<MinecraftSession>? SessionCreated;

    public LaunchService(
        SettingsService settingsService,
        GameService gameService,
        AccountService accountService,
        AuthenticationService authenticationService,
        DownloadService downloadService)
    {
        _accountService = accountService;
        _authenticationService = authenticationService;
        _settingsService = settingsService;
        _downloadService = downloadService;
        _gameService = gameService;

        Sessions = new(_sessions);
    }

    public async void LaunchGame(GameInfo gameInfo)
    {
        var session = CreateLaunchSession(gameInfo); // TODO: replace with ctor of MinecraftSession
        _sessions.Add(session);
        SessionCreated?.Invoke(this, session);

        var specialConfig = gameInfo.GetSpecialConfig();

        // Update launch time
        var launchTime = DateTime.Now;
        specialConfig.LastLaunchTime = launchTime;

        var contained = _gameService.GameInfos.Where(x => x.AbsoluteId.Equals(gameInfo.AbsoluteId)).FirstOrDefault();
        if (contained != null)
            contained.LastLaunchTime = launchTime;

        if (gameInfo is ExtendedGameInfo extendedGameInfo)
            extendedGameInfo.LastLaunchTime = launchTime;

        UpdateJumpList(gameInfo);

        try
        {
            await session.StartAsync(); // TODO: update to a fully async implementation
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void LaunchFromJumpList(string arguments)
    {
        //TODO: Implement launch from jump list
        //var _gameInfo = JsonSerializer.Deserialize<GameInfo>(arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64());

        //new ToastContentBuilder()
        //    .AddHeader(_gameInfo.Name, $"正在尝试启动游戏: {_gameInfo.Name}", string.Empty)
        //    .AddText("这可能需要一点时间，请稍后")
        //    .Show();

        //var process = CreateLaunchSession(_gameInfo);
        //_launchProcesses.Insert(0, process);

        //Task.Run(process.Start);

        //process.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
        //{
        //    if (e.PropertyName == "DisplayState")
        //    {
        //        if (!(process.State == MinecraftSessionState.GameRunning ||
        //            process.State == MinecraftSessionState.Faulted ||
        //            process.State == MinecraftSessionState.GameExited ||
        //            process.State == MinecraftSessionState.GameCrashed))
        //            return;

        //        var builder = new ToastContentBuilder();

        //        switch (process.State)
        //        {
        //            case MinecraftSessionState.GameRunning:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动游戏成功: {_gameInfo.Name}", string.Empty);
        //                break;
        //            case MinecraftSessionState.Faulted:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动游戏失败: {_gameInfo.Name}", string.Empty);
        //                break;
        //            case MinecraftSessionState.GameExited:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动进程消息: {_gameInfo.Name}", string.Empty);
        //                break;
        //            case MinecraftSessionState.GameCrashed:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动进程消息: {_gameInfo.Name}", string.Empty);
        //                break;
        //            default:
        //                break;
        //        }

        //        builder.AddText(ResourceUtils.GetValue("Converters", $"_LaunchState_{process.State}"));
        //        builder.Show();

        //        switch (process.State)
        //        {
        //            case MinecraftSessionState.Faulted:
        //            case MinecraftSessionState.GameExited:
        //                Process.GetCurrentProcess().Kill();
        //                break;
        //            case MinecraftSessionState.GameCrashed:
        //                App.DispatcherQueue.TryEnqueue(() => process.LoggerButton());
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //};
    }

    public MinecraftSession CreateLaunchSession(GameInfo gameInfo)
    {
        // Java
        string? suitableJava = null;

        if (string.IsNullOrEmpty(_settingsService.ActiveJava))
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoActiveJava")); 
        // TODO: Do not localize exception message

        suitableJava = _settingsService.EnableAutoJava ? GetSuitableJava(gameInfo) : _settingsService.ActiveJava;
        if (suitableJava == null)
            throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoSuitableJava").Replace("${version}", gameInfo.GetSuitableJavaVersion()));

        var specialConfig = gameInfo.GetSpecialConfig(); // Game specific config
        var launchAccount = GetLaunchAccount(specialConfig, _accountService) 
            ?? throw new Exception(ResourceUtils.GetValue("Exceptions", "_NoAccount")); // Determine which account to use

        Account Authenticate()
        {
            // TODO: refactor to remove dependency on AuthenticationService, and AccountService.
            // Call FluentCore to refresh account directly.
            try
            {
                if (launchAccount.Equals(_accountService.ActiveAccount))
                {
                    _authenticationService.RefreshCurrentAccount();
                    return _accountService.ActiveAccount;
                }
                else
                {
                    _authenticationService.RefreshContainedAccount(launchAccount);
                    return GetLaunchAccount(specialConfig, _accountService);
                }
            }
            catch (Exception ex)
            {
                throw new AuthenticateRefreshAccountException(ex);
            }
        }

        var (maxMemory, minMemory) = _settingsService.EnableAutoJava
            ? MemoryUtils.CalculateJavaMemory()
            : (_settingsService.JavaMemory, _settingsService.JavaMemory);

        var session = new MinecraftSession() // Launch session
        {
            Account = launchAccount,
            GameInfo = gameInfo,
            GameDirectory = GetGameDirectory(gameInfo, specialConfig),
            JavaPath = suitableJava,
            MaxMemory = maxMemory,
            MinMemory = minMemory,
            UseDemoUser = _settingsService.EnableDemoUser,
            ExtraGameParameters = GetExtraGameParameters(specialConfig),
            ExtraVmParameters = GetExtraVmParameters(specialConfig, launchAccount),
            CreateResourcesDownloader = (libs) => _downloadService.CreateResourcesDownloader
                (gameInfo, libs)
        };

        if (_settingsService.AutoRefresh)
            session.RefreshAccountTask = new Task<Account>(Authenticate);

        session.ProcessStarted += (s, e) =>
        {
            var title = GameWindowTitle(specialConfig);
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

    private string GetGameDirectory(GameInfo gameInfo, GameSpecialConfig specialConfig)
    {
        if (specialConfig.EnableSpecialSetting)
        {
            if (specialConfig.EnableIndependencyCore)
                return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);
            else return gameInfo.MinecraftFolderPath;
        }

        if (_settingsService.EnableIndependencyCore)
            return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);

        return gameInfo.MinecraftFolderPath;
    }

    private string? GameWindowTitle(GameSpecialConfig specialConfig)
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

    public static Account GetLaunchAccount(GameSpecialConfig specialConfig, AccountService _accountService)
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

    private IEnumerable<string> GetExtraVmParameters(GameSpecialConfig specialConfig, Account account)
    {
        if (account is YggdrasilAccount yggdrasil)
        {
            using var res = HttpUtils.HttpGet(yggdrasil.YggdrasilServerUrl);

            yield return $"-javaagent:{Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "Libs", "authlib-injector-1.2.3.jar").ToPathParameter()}={yggdrasil.YggdrasilServerUrl}";
            yield return "-Dauthlibinjector.side=client";
            yield return $"-Dauthlibinjector.yggdrasil.prefetched={(res.Content.ReadAsString()).ConvertToBase64()}";
        }

        if (!specialConfig.EnableSpecialSetting || specialConfig.VmParameters == null)
            yield break;

        foreach (var item in specialConfig.VmParameters)
            yield return item;
    }

    private IEnumerable<string> GetExtraGameParameters(GameSpecialConfig specialConfig)
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

    private static void UpdateJumpList(GameInfo gameInfo) => Task.Run(async () =>
    {
        var jumpList = await JumpList.LoadCurrentAsync();

        var args = JsonSerializer.Serialize(gameInfo).ConvertToBase64();
        var latest = jumpList.Items.Where(x =>
        {
            var gameInfo = JsonSerializer.Deserialize<GameInfo>(x.Arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64())
                ?? throw new Exception("Cannot deserialize json");

            return x.GroupName.Equals("Latest") &&
            Path.Exists(Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId, $"{gameInfo.AbsoluteId}.json"));
        }).Take(6).ToList();

        var jumpListItem = JumpListItem.CreateWithArguments($"/quick-launch {args}", gameInfo.Name);

        jumpListItem.Logo = new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", !gameInfo.IsVanilla ? "furnace_front" : gameInfo.Type switch
        {
            "release" => "grass_block_side",
            "snapshot" => "crafting_table_front",
            "old_beta" => "dirt_path_side",
            "old_alpha" => "dirt_path_side",
            _ => "grass_block_side"
        }), UriKind.RelativeOrAbsolute);

        jumpListItem.GroupName = "Latest";

        if (latest.Where(x => x.DisplayName.Equals(gameInfo.Name)).FirstOrDefault() is JumpListItem equalsItem)
            latest.Remove(equalsItem);

        latest.Insert(0, jumpListItem);
        jumpList.Items.Clear();

        foreach (var item in latest)
            jumpList.Items.Add(item);

        await jumpList.SaveAsync();
    });
}

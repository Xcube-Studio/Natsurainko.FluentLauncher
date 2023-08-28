using CommunityToolkit.WinUI.Notifications;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Classes.Exceptions;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.DefaultComponents.Launch;
using Nrk.FluentCore.DefaultComponents.Parse;
using Nrk.FluentCore.Services.Launch;
using Nrk.FluentCore.Utils;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.StartScreen;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class LaunchService : DefaultLaunchService
{
    private readonly AuthenticationService _authenticationService;
    private readonly new SettingsService _settingsService;
    private readonly new AccountService _accountService;
    private readonly new GameService _gameService;
    private readonly ObservableCollection<LaunchProcess> _launchProcesses = new();
    private readonly DownloadService _downloadService;

    public ReadOnlyObservableCollection<LaunchProcess> LaunchProcesses { get; init; }

    public LaunchService(
        SettingsService settingsService,
        GameService gameService,
        AccountService accountService,
        AuthenticationService authenticationService,
        DownloadService downloadService)
        : base(settingsService, gameService, accountService)
    {
        _accountService = accountService;
        _authenticationService = authenticationService;
        _settingsService = settingsService;
        _downloadService = downloadService;
        _gameService = gameService;

        LaunchProcesses = new(_launchProcesses);
    }

    public void LaunchNewGame(GameInfo gameInfo)
    {
        var process = CreateLaunchProcess(gameInfo);
        _launchProcesses.Insert(0, process);

        Task.Run(process.RunLaunch);
        Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.LaunchPage));
    }

    public void LaunchFromJumpList(string arguments)
    {
        var gameInfo = JsonSerializer.Deserialize<GameInfo>(arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64());

        new ToastContentBuilder()
            .AddHeader(gameInfo.Name, $"正在尝试启动游戏: {gameInfo.Name}", string.Empty)
            .AddText("这可能需要一点时间，请稍后")
            .Show();

        var process = CreateLaunchProcess(gameInfo);
        _launchProcesses.Insert(0, process);

        Task.Run(process.RunLaunch);
        Views.ShellPage.ContentFrame?.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.LaunchPage));

        process.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
        {
            if (e.PropertyName == "DisplayState")
            {
                if (!(process.State == LaunchState.GameRunning ||
                    process.State == LaunchState.Faulted ||
                    process.State == LaunchState.GameExited ||
                    process.State == LaunchState.GameCrashed))
                    return;

                var builder = new ToastContentBuilder();

                switch (process.State)
                {
                    case LaunchState.GameRunning:
                        builder.AddHeader(Guid.NewGuid().ToString(), $"启动游戏成功: {gameInfo.Name}", string.Empty);
                        break;
                    case LaunchState.Faulted:
                        builder.AddHeader(Guid.NewGuid().ToString(), $"启动游戏失败: {gameInfo.Name}", string.Empty);
                        break;
                    case LaunchState.GameExited:
                        builder.AddHeader(Guid.NewGuid().ToString(), $"启动进程消息: {gameInfo.Name}", string.Empty);
                        break;
                    case LaunchState.GameCrashed:
                        builder.AddHeader(Guid.NewGuid().ToString(), $"启动进程消息: {gameInfo.Name}", string.Empty);
                        break;
                    default:
                        break;
                }

                builder.AddText(ResourceUtils.GetValue("Converters", $"_LaunchState_{process.State}"));
                builder.Show();

                switch (process.State)
                {
                    case LaunchState.Faulted:
                    case LaunchState.GameExited:
                        Process.GetCurrentProcess().Kill();
                        break;
                    case LaunchState.GameCrashed:
                        App.DispatcherQueue.TryEnqueue(() => process.LoggerButton());
                        break;
                    default:
                        break;
                }
            }
        };
    }

    public new LaunchProcess CreateLaunchProcess(GameInfo gameInfo)
    {
        var libraryParser = new DefaultLibraryParser(gameInfo);
        libraryParser.EnumerateLibraries(out var enabledLibraries, out var enabledNativesLibraries);

        var specialConfig = gameInfo.GetSpecialConfig();

        string suitableJava = default;
        string gameDirectory = GetGameDirectory(gameInfo, specialConfig);
        var launchAccount = GetLaunchAccount(specialConfig);

        var launchProcess = new LaunchProcessBuilder(gameInfo)
            .SetInspectAction(() =>
            {
                if (_settingsService.ActiveJava == null) return false;
                if (launchAccount == null) return false;

                suitableJava = GetSuitableJava(gameInfo);
                if (suitableJava == null) return false;

                return true;
            })
            .SetAuthenticateFunc(() =>
            {
                try
                {
                    if (_settingsService.AutoRefresh)
                    {
                        if (launchAccount.Equals(_accountService.ActiveAccount))
                        {
                            _authenticationService.RefreshCurrentAccount();
                            launchAccount = _accountService.ActiveAccount;
                        }
                        else
                        {
                            _authenticationService.RefreshContainedAccount(launchAccount);
                            launchAccount = GetLaunchAccount(specialConfig);
                        }
                    }
                }
                catch(Exception ex) 
                {
                    throw new AuthenticateRefreshAccountException(ex);
                }
            })
            .SetCompleteResourcesAction(launchProcess =>
            {
                var resourcesDownloader = _downloadService.CreateResourcesDownloader(gameInfo, enabledLibraries.Union(enabledNativesLibraries));
                resourcesDownloader.SingleFileDownloaded += (_, _) => App.DispatcherQueue.TryEnqueue(launchProcess.UpdateDownloadProgress);
                resourcesDownloader.DownloadElementsPosted += (_, count) => App.DispatcherQueue.TryEnqueue(() =>
                {
                    launchProcess.StepItems[2].TaskNumber = count;
                    launchProcess.UpdateLaunchProgress();
                });

                resourcesDownloader.Download();

                if (resourcesDownloader.ErrorDownload.Count > 0)
                    throw new CompleteGameResourcesException(resourcesDownloader);

                UnzipUtils.BatchUnzip(
                    Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId, "natives"),
                    enabledNativesLibraries.Select(x => x.AbsolutePath));
            })
            .SetBuildArgumentsFunc(() =>
            {
                (int maxMemory, int minMemory) = MemoryUtils.CalculateJavaMemory(_settingsService.JavaMemory);

                var builder = new DefaultArgumentsBuilder(gameInfo)
                    .SetLibraries(enabledLibraries)
                    .SetAccountSettings(launchAccount, _settingsService.EnableDemoUser)
                    .SetJavaSettings(suitableJava, maxMemory, minMemory)
                    .SetGameDirectory(gameDirectory)
                    .AddExtraParameters(GetExtraVmParameters(specialConfig, launchAccount), GetExtraGameParameters(specialConfig));

                return builder.Build();
            })
            .SetCreateProcessFunc(() =>
            {
                var launchTime = DateTime.Now;

                specialConfig.LastLaunchTime = launchTime;
                _gameService.GameInfos.Where(x => x.AbsoluteId.Equals(gameInfo.AbsoluteId)).FirstOrDefault().LastLaunchTime = launchTime;

                if (gameInfo is ExtendedGameInfo extendedGameInfo)
                    extendedGameInfo.LastLaunchTime = launchTime;

                UpdateJumpList(gameInfo);

                return new Process
                {
                    StartInfo = new ProcessStartInfo(suitableJava)
                    {
                        WorkingDirectory = gameDirectory
                    },
                };
            })
            .Build();

        launchProcess.GameProcessStart += (_, _) =>
        {
            var title = GameWindowTitle(specialConfig);
            if (string.IsNullOrEmpty(title)) return;

            Task.Run(async () =>
            {
                try
                {
                    while (!(launchProcess.McProcess?.HasExited).GetValueOrDefault(true))
                    {
                        if (launchProcess.McProcess != null && launchProcess.McProcess?.MainWindowTitle != title)
                            User32.SetWindowText(launchProcess.McProcess.MainWindowHandle, title);

                        await Task.Delay(1000);
                        launchProcess.McProcess?.Refresh();
                    }
                }
                catch //(Exception ex)
                {
                    //throw;
                }
            });
        };

        return launchProcess;
    }

    private string GetSuitableJava(GameInfo gameInfo)
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

        if (!suits.Any()) return null;

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

    private string GameWindowTitle(GameSpecialConfig specialConfig)
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

    private Account GetLaunchAccount(GameSpecialConfig specialConfig)
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

    private async void UpdateJumpList(GameInfo gameInfo)
    {
        var jumpList = await JumpList.LoadCurrentAsync();

        var args = JsonSerializer.Serialize(gameInfo).ConvertToBase64();
        var latest = jumpList.Items.Where(x => x.GroupName.Equals("Latest")).ToList();

        if (!latest.Where(x => x.DisplayName.Equals(gameInfo.Name)).Any())
        {
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

            jumpList.Items.Add(jumpListItem);

        }

        await jumpList.SaveAsync();
    }
}

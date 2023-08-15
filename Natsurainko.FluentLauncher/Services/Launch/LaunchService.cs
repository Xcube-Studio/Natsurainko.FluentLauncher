using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.DefaultComponents.Launch;
using Nrk.FluentCore.DefaultComponents.Parse;
using Nrk.FluentCore.Services.Launch;
using Nrk.FluentCore.Utils;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;

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
            })
            .SetCompleteResourcesAction(launchProcess =>
            {
                var resourcesDownloader = _downloadService.CreateResourcesDownloader(gameInfo, enabledLibraries.Union(enabledNativesLibraries));
                resourcesDownloader.SingleFileDownloaded += (_, _) => App.MainWindow.DispatcherQueue.TryEnqueue(launchProcess.UpdateDownloadProgress);
                resourcesDownloader.DownloadElementsPosted += (_, count) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    launchProcess.StepItems[2].TaskNumber = count;
                    launchProcess.UpdateLaunchProgress();
                });

                resourcesDownloader.Download();

                if (resourcesDownloader.ErrorDownload.Count > 0)
                    throw new Exception("ResourcesDownloader.ErrorDownload.Count > 0");

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
                    .AddExtraParameters(GetExtraVmParameters(specialConfig, launchAccount), GetExtraGameParameters(specialConfig)); //TODO: 加入额外的虚拟机参数  如：-XX:+UseG1GC 以及额外游戏参数 如：--fullscreen

                return builder.Build();
            })
            .SetCreateProcessFunc(() =>
            {
                var launchTime = DateTime.Now;

                specialConfig.LastLaunchTime = launchTime;
                _gameService.GameInfos.Where(x => x.Equals(gameInfo)).FirstOrDefault().LastLaunchTime = launchTime;
                (gameInfo as ExtendedGameInfo).LastLaunchTime = launchTime;

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
}

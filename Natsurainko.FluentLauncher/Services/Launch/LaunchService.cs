using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.DefaultComponets.Launch;
using Nrk.FluentCore.DefaultComponets.Parse;
using Nrk.FluentCore.Services.Launch;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class LaunchService : DefaultLaunchService
{
    private readonly AuthenticationService _authenticationService;
    private readonly new SettingsService _settingsService;
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
        _authenticationService = authenticationService;
        _settingsService = settingsService;
        _downloadService = downloadService;

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

        string suitableJava = default;
        string gameDirectory = GetGameDirectory(gameInfo);

        var launchProcess = new LaunchProcessBuilder(gameInfo)
            .SetInspectAction(() =>
            {
                if (_settingsService.ActiveJava == null) return false;
                if (_accountService.ActiveAccount == null) return false;

                suitableJava = GetSuitableJava(gameInfo);
                if (suitableJava == null) return false;

                return true;
            })
            .SetAuthenticateFunc(() => 
            {
                if (_settingsService.AutoRefresh) _authenticationService.RefreshCurrentAccount();
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
                    .SetAccountSettings(_accountService.ActiveAccount, _settingsService.EnableDemoUser)
                    .SetJavaSettings(suitableJava, maxMemory, minMemory)
                    .SetGameDirectory(gameDirectory);
                    //.AddExtraParameters(); TODO: 加入额外的虚拟机参数  如：-XX:+UseG1GC 以及额外游戏参数 如：--fullscreen

                return builder.Build();
            })
            .SetCreateProcessFunc(() => new Process
            {
                StartInfo = new ProcessStartInfo(suitableJava)
                {
                    WorkingDirectory = gameDirectory
                },
            })
            .Build();

        launchProcess._suitableJava = suitableJava;
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

    private string GetGameDirectory(GameInfo gameInfo)
    {
        string gameDirectory = gameInfo.MinecraftFolderPath;
        string independence = Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);

        var specialConfig = gameInfo.GetSpecialConfig();

        if (specialConfig.EnableSpecialSetting)
            gameDirectory = specialConfig.EnableIndependencyCore ? independence : gameDirectory;
        else gameDirectory = _settingsService.EnableIndependencyCore ? independence : gameDirectory;

        return gameDirectory;
    }
}

using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Classes.Exceptions;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Management.Parsing;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Lights;

namespace Natsurainko.FluentLauncher.Services.Launch;
#nullable enable

enum MinecraftSessionState
{
    Created = 0,
    Inspecting = 1,
    Authenticating = 2,
    CompletingResources = 3,
    BuildingArguments = 4,
    LaunchingProcess = 5,
    GameRunning = 6,
    GameExited = 7,
    Faulted = 8,
    Killed = 9,
    GameCrashed = 10
}

// Encapsulates a launch session, holds a MinecraftProcess instance
class MinecraftSession
{
    private readonly DownloadService _downloadService;
    private readonly AuthenticationService _authenticationService;
    private readonly AccountService _accountService;

    /// <summary>
    /// The Minecraft game instance to launch
    /// </summary>
    public GameInfo GameInfo { get; }

    public GameSpecialConfig SpecialConfig { get; }

    /// <summary>
    /// Account to use for Minecraft
    /// </summary>
    public Account Account { get; private set; }

    public bool EnableAccountRefresh { get; init; }

    public MinecraftProcess? MinecraftProcess { get; private set; }

    public MinecraftSessionState State { get; private set; } = MinecraftSessionState.Created;

    public event EventHandler? DownloadProgressChagned;

    public event EventHandler? SingleFileDownloaded;

    public event EventHandler<int>? DownloadElementsPosted;


    private readonly IEnumerable<LibraryElement> _enabledLibraries;
    private readonly string _javaPath;
    private readonly bool _useDemoUser;
    private readonly int _javaMemory;
    private readonly string _gameDirectory;
    private readonly IEnumerable<string> _vmParameters;
    private readonly IEnumerable<string> _extraGameParameters;

    public MinecraftSession(GameInfo gameInfo, Account account, GameSpecialConfig specialConfig, IEnumerable<LibraryElement> libraries, string javaPath, bool useDemoUser, int javaMemory, string gameDirectory,
        IEnumerable<string> vmParameters, IEnumerable<string> extraGameParameters,
        DownloadService downloadService, AuthenticationService authenticationService, AccountService accountService)
    {
        GameInfo = gameInfo;
        Account = account;
        SpecialConfig = specialConfig;
        _enabledLibraries = libraries;
        _javaPath = javaPath;
        _useDemoUser = useDemoUser;
        _javaMemory = javaMemory;
        _gameDirectory = gameDirectory;
        _vmParameters = vmParameters;
        _extraGameParameters = extraGameParameters;

        _downloadService = downloadService;
        _authenticationService = authenticationService;
        _accountService = accountService;
    }

    /// <summary>
    /// Start the Minecraft game (guarantees <see cref="MinecraftProcess"/> is not null when finished)
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        // QUESTION: Use async or not?
        State = MinecraftSessionState.Inspecting;
        await Task.Run(CheckEnvironment);

        State = MinecraftSessionState.Authenticating;
        var authTask = Task.Run(Authenticate);

        State = MinecraftSessionState.CompletingResources;
        var depTask = Task.Run(CheckAndCompleteDependencies);

        State = MinecraftSessionState.BuildingArguments;
        MinecraftProcess = CreateMinecraftProcess();

        State = MinecraftSessionState.LaunchingProcess;
        await Task.Run(MinecraftProcess.Start);
        State = MinecraftSessionState.GameRunning;
    }

    // Check if Java path provided is valid
    private void CheckEnvironment()
    {
        // TODO: check memory?
        if (!File.Exists(_javaPath))
            throw new FileNotFoundException(_javaPath);
    }

    // Refresh the account if auto refresh is enabled
    private void Authenticate()
    {
        // TODO: refactor to remove dependency on LaunchService, AuthenticationService, and AccountService.
        // Call FluentCore to refresh account directly.
        try
        {
            if (EnableAccountRefresh)
            {
                if (Account.Equals(_accountService.ActiveAccount))
                {
                    _authenticationService.RefreshCurrentAccount();
                    Account = _accountService.ActiveAccount;
                }
                else
                {
                    _authenticationService.RefreshContainedAccount(Account);
                    Account = LaunchService.GetLaunchAccount(SpecialConfig, _accountService);
                }
            }
        }
        catch (Exception ex)
        {
            throw new AuthenticateRefreshAccountException(ex);
        }
    }

    // Check dependencies and download if needed
    private void CheckAndCompleteDependencies()
    {
        var libraryParser = new DefaultLibraryParser(GameInfo);
        libraryParser.EnumerateLibraries(
            out var enabledLibraries,
            out var enabledNativesLibraries
        );

        var resourcesDownloader = _downloadService.CreateResourcesDownloader(GameInfo, enabledLibraries.Union(enabledNativesLibraries));
        resourcesDownloader.SingleFileDownloaded += (_, e) =>
            SingleFileDownloaded?.Invoke(resourcesDownloader, e);
        resourcesDownloader.DownloadElementsPosted += (_, count) =>
            DownloadElementsPosted?.Invoke(resourcesDownloader, count);
        // TODO: subscribe events in view model
        //resourcesDownloader.SingleFileDownloaded += (_, _) => App.DispatcherQueue.TryEnqueue(launchProcess.UpdateDownloadProgress);
        //resourcesDownloader.DownloadElementsPosted += (_, count) => App.DispatcherQueue.TryEnqueue(() =>
        //{
        //    launchProcess.StepItems[2].TaskNumber = count;
        //    launchProcess.UpdateLaunchProgress();
        //});

        resourcesDownloader.Download();
        if (resourcesDownloader.ErrorDownload.Count > 0)
            throw new CompleteGameResourcesException(resourcesDownloader);

        UnzipUtils.BatchUnzip(
            Path.Combine(GameInfo.MinecraftFolderPath, "versions", GameInfo.AbsoluteId, "natives"),
            enabledNativesLibraries.Select(x => x.AbsolutePath));
    }

    private MinecraftProcess CreateMinecraftProcess()
    {
        (int maxMemory, int minMemory) = MemoryUtils.CalculateJavaMemory(_javaMemory);

        var args = new DefaultArgumentsBuilder(GameInfo)
            .SetLibraries(_enabledLibraries)
            .SetAccountSettings(Account, _useDemoUser)
            .SetJavaSettings(_javaPath, maxMemory, minMemory)
            .SetGameDirectory(_gameDirectory)
            .AddExtraParameters(_vmParameters, _extraGameParameters)
            .Build();

        return new MinecraftProcess(_javaPath, _gameDirectory, args);
    }
}

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
using System.Diagnostics;
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

class MinecraftSessionStateChagnedEventArgs : EventArgs
{
    public MinecraftSessionState OldState { get; }
    public MinecraftSessionState NewState { get; }

    public MinecraftSessionStateChagnedEventArgs(MinecraftSessionState oldState, MinecraftSessionState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}

// Encapsulates a launch session, holds a _mcProcess instance
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

    private MinecraftSessionState _state = MinecraftSessionState.Created;

    public MinecraftSessionState State
    {
        get { return _state; }
        private set
        {
            var old = _state;
            _state = value;
            StateChanged?.Invoke(this, new MinecraftSessionStateChagnedEventArgs(old, value));
        }
    }

    public event EventHandler? SingleFileDownloaded;
    public event EventHandler<int>? DownloadElementsPosted;

    public event EventHandler? ProcessStarted;
    public event EventHandler<MinecraftProcessExitedEventArgs>? ProcessExited;

    /// <summary>
    /// Raised when <see cref="State"/> changes
    /// </summary>
    public event EventHandler<MinecraftSessionStateChagnedEventArgs>? StateChanged;

    public event DataReceivedEventHandler? OutputDataReceived;
    public event DataReceivedEventHandler? ErrorDataReceived;

    private MinecraftProcess? _mcProcess; // TODO: Create on init, so it can be non-nullable, update argument list when needed before the process is started
    // QUESTION: Should this be public? MinecraftSession is designed to hide the underlying process, maybe should forward events instead?

    private readonly IEnumerable<LibraryElement> _enabledLibraries;
    private readonly string _javaPath;
    private readonly bool _useDemoUser;
    private readonly int _javaMemory;
    private readonly string _gameDirectory;
    private readonly IEnumerable<string> _vmParameters;
    private readonly IEnumerable<string> _extraGameParameters;

    // Sets to true when a Kill() is called, used for the Exited event handler to deterine state
    private bool _killRequested = false;

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
    /// Start the Minecraft game (guarantees <see cref="_mcProcess"/> is not null when finished)
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        try
        {
            // QUESTION: Use async or not?
            State = MinecraftSessionState.Inspecting;
            CheckEnvironment();

            State = MinecraftSessionState.Authenticating;
            var authTask = Task.Run(Authenticate);

            State = MinecraftSessionState.CompletingResources;
            var depTask = Task.Run(CheckAndCompleteDependencies);

            State = MinecraftSessionState.BuildingArguments;
            _mcProcess = CreateMinecraftProcess();

            // Forward events from _mcProcess
            _mcProcess.OutputDataReceived += OutputDataReceived;
            _mcProcess.ErrorDataReceived += ErrorDataReceived;
            _mcProcess.Started += ProcessStarted;
            _mcProcess.Exited += ProcessExited;
        }
        catch (Exception)
        {
            State = MinecraftSessionState.Faulted;
            // QUESTION: Invoke an event here? Maybe just use a general state changed event?
            throw; // rethrow for caller to handle
        }

        State = MinecraftSessionState.LaunchingProcess;
        await Task.Run(_mcProcess.Start);
        State = MinecraftSessionState.GameRunning;

        // Updates session state when the game process exits
        _mcProcess.Exited += (_, e) =>
        {
            if (e.ExitCode == 0)
            {
                State = MinecraftSessionState.GameExited;
            }
            else if (_killRequested)
            {
                State = MinecraftSessionState.Killed;
                _killRequested = false;
            }
            else
            {
                State = MinecraftSessionState.GameCrashed;
            }
            _mcProcess.Dispose(); // Release resources used by the Minecraft process when it exits. Exit code is reflected by the MinecraftSessionState.
        };
    }

    /// <summary>
    /// Kills a running Minecraft game. The operation is invalid if the game is not running.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Kill()
    {
        // TODO: Cancel the launch sequence when an async version is implemented
        if (_mcProcess is null)
            throw new InvalidOperationException("Process has not been created");

        _killRequested = true;
        _mcProcess.Kill();
        // State change handeled by _mcProcess.Exited event
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
        // TODO: refactor to remove dependency on AuthenticationService, and AccountService.
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

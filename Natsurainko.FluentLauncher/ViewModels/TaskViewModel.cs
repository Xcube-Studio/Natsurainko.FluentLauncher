using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Notification;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Exceptions;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views;
using Nrk.FluentCore.Exceptions;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Natsurainko.FluentLauncher.Services.Launch.LaunchProgress;
using static Natsurainko.FluentLauncher.ViewModels.LaunchStageProgress;

using Timer = System.Timers.Timer;

namespace Natsurainko.FluentLauncher.ViewModels;

internal abstract partial class TaskViewModel : ObservableObject
{
    private readonly CancellationTokenSource _tokenSource = new();
    protected readonly Stopwatch _stopwatch = new();

    protected virtual string StopwatchFormat => "hh\\:mm\\:ss";

    protected virtual TimeSpan TimerSpan => TimeSpan.FromSeconds(1);

    protected abstract ILogger Logger { get; }

    protected Timer Timer { get; private set; } = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    public partial TaskState TaskState { get; set; } = TaskState.Prepared;

    #region Basic Properties

    public abstract string Icon { get; }

    public abstract string Title { get; }

    public string TimeUsage => _stopwatch.Elapsed.ToString(StopwatchFormat);

    #endregion

    #region Task Properties

    protected Task ExecuteTask { get; private set; } = null!;

    public bool IsFaulted => ExecuteTask.IsFaulted;

    public bool IsCanceled => ExecuteTask.IsCanceled;

    public bool IsCompleted => ExecuteTask.IsCompleted;

    public virtual string? ExceptionContent => ExecuteTask!.Exception?.ToString();

    public virtual string? ExceptionTitle => ExecuteTask!.Exception?.GetType().FullName;

    #endregion

    #region UI Properties

    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressBarPercentageText))]
    [NotifyPropertyChangedFor(nameof(ProgressBarIsIndeterminate))]
    public partial double Progress { get; set; } = 0;

    public string ProgressBarPercentageText => $"{Progress:P1}";

    public virtual bool ProgressBarIsIndeterminate => Progress <= 0;

    public virtual string? InfoBarTitle => ExecuteTask!.Exception?.Message;

    #endregion

    public virtual bool CanCancel => TaskState == TaskState.Running;

    public virtual async Task EnqueueAsync()
    {
        Timer = CreateTimer();
        Timer.Start();

        _stopwatch.Start();

        Logger.TaskEnqueued();

        ExecuteTask = Task.Run(async () => await ExecuteAsync(_tokenSource.Token), _tokenSource.Token);
        await App.DispatcherQueue.EnqueueAsync(() => TaskState = TaskState.Running);

        ExecuteTask.ContinueWith(t =>
        {
            _stopwatch.Stop();

            Timer.Stop();
            Timer.Dispose();

            App.DispatcherQueue.TryEnqueue(() =>
            {
                Progress = 1;

                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(IsCanceled));
                OnPropertyChanged(nameof(IsFaulted));

                if (IsFaulted)
                {
                    OnPropertyChanged(nameof(ExceptionTitle));
                    OnPropertyChanged(nameof(ExceptionContent));

                    TaskState = TaskState.Failed;
                    Logger.TaskFaulted(t.Exception);

                    NotifyException(App.GetService<INotificationService>());
                }
                else if (IsCanceled || _tokenSource.IsCancellationRequested)
                {
                    TaskState = TaskState.Cancelled;
                    Logger.TaskCancelld();
                }
                else
                {
                    TaskState = TaskState.Finished;
                    Logger.TaskRanToCompletion();
                }
            });

            WeakReferenceMessenger.Default.Send(new BackgroundTaskCountChangedMessage());
        }).Forget();

        WeakReferenceMessenger.Default.Send(new BackgroundTaskCountChangedMessage());
    }

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

    [RelayCommand(CanExecute = nameof(CanCancel))]
    public async Task Cancel()
    {
        TaskState = TaskState.Canceling;
        await _tokenSource.CancelAsync();
    }

    [RelayCommand]
    void CopyExceptionContent()
    {
        if (ExceptionContent != null)
        {
            App.GetService<INotificationService>().ExceptionCopied();
            ClipboardHepler.SetText(ExceptionContent);
        }
    }

    protected virtual Timer CreateTimer()
    {
        TimeSpan timerSpan = TimeSpan.FromSeconds(1);

        Timer timer = new(timerSpan);
        timer.Elapsed += Timer_Elapsed;

        return timer;
    }

    protected virtual void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        => App.DispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(TimeUsage)));

    protected virtual void NotifyException(INotificationService notificationService) { }
}

#region Download Resource Task

internal partial class DownloadResourceTaskViewModel : TaskViewModel
{
    private readonly DownloadService _downloadService;

    private readonly string _filePath;
    private readonly Task<string> _getUrlTask;
    private readonly Action<string>? _downloadedAction;

    private DownloadTask? DownloadTask { get; set; }

    protected override ILogger Logger { get; } = App.GetService<ILogger<DownloadResourceTaskViewModel>>();

    public DownloadResourceTaskViewModel(DownloadService downloadService, CurseForgeFile curseForgeFile, string folder, Action<string>? action = null)
    {
        _downloadService = downloadService;
        _filePath = Path.Combine(folder, curseForgeFile.FileName);
        _getUrlTask = Task.Run(() => App.GetService<CurseForgeClient>().GetFileUrlAsync(curseForgeFile));
        _downloadedAction = action;
    }

    public DownloadResourceTaskViewModel(DownloadService downloadService, ModrinthFile modrinthFile, string folder, Action<string>? action = null)
    {
        _downloadService = downloadService;
        _filePath = Path.Combine(folder, modrinthFile.FileName);
        _getUrlTask = Task.FromResult(modrinthFile.Url);
        _downloadedAction = action;
    }

    public DownloadResourceTaskViewModel(DownloadService downloadService, string url, string filePath, Action<string>? action = null)
    {
        _downloadService = downloadService;
        _filePath = filePath;
        _getUrlTask = Task.FromResult(url);
        _downloadedAction = action;
    }

    #region Basic Properties

    public override string Title => Path.GetFileName(_filePath);

    public override string Icon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\uE896",
    };

    #endregion

    #region Download Properties

    [ObservableProperty]
    public partial bool Downloaded { get; set; }

    public string DownloadedBytes => LongExtensions.ToFileSizeString(DownloadTask?.DownloadedBytes);

    public string TotalBytes => LongExtensions.ToFileSizeString(DownloadTask?.TotalBytes);

    #endregion

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DownloadTask = _downloadService.Downloader.CreateDownloadTask(await _getUrlTask, _filePath);
        var downloadResult = await DownloadTask.StartAsync(cancellationToken);

        if (downloadResult.Type != DownloadResultType.Successful)
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            cancellationToken.ThrowIfCancellationRequested();
            throw downloadResult.Exception!;
        }

        await App.DispatcherQueue.EnqueueAsync(() =>
        {
            Downloaded = true;
            OnPropertyChanged(nameof(DownloadedBytes));
            OnPropertyChanged(nameof(TotalBytes));
        });

        _downloadedAction?.Invoke(_filePath);
    }

    [RelayCommand]
    void OpenFolder() => ExplorerHelper.ShowAndSelectFile(_filePath);

    [RelayCommand]
    void CopyUrl()
    {
        if (DownloadTask == null) return;

        ClipboardHepler.SetText(DownloadTask.Request.Url);
        App.GetService<INotificationService>().DownloadUrlCopied();
    }

    [RelayCommand]
    void Retry()
    {
        if (DownloadTask == null)
            return;

        _downloadService.DownloadResourceFileAsync(DownloadTask.Request.Url, _filePath, _downloadedAction).Forget();

        Remove();
    }

    [RelayCommand]
    void Remove() => _downloadService.DownloadTasks.Remove(this);

    #region Exception

    public override string InfoBarTitle => LocalizedStrings.Notifications__TaskFailed_ResourceDownload;

    #endregion

    #region Timer Override

    protected override void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        base.Timer_Elapsed(sender, e);

        double progress = 0;

        if (DownloadTask?.TotalBytes != null)
            progress = DownloadTask.DownloadedBytes / (double)DownloadTask.TotalBytes;

        App.DispatcherQueue.TryEnqueue(() =>
        {
            Progress = progress;
            OnPropertyChanged(nameof(DownloadedBytes));
            OnPropertyChanged(nameof(TotalBytes));
        });
    }

    #endregion
}

#endregion

#region Install Instance Task

class InstallationViewModel<TStage> : IProgress<IInstallerProgress>
    where TStage : struct, Enum
{
    public Dictionary<TStage, InstallationStageViewModel> Stages { get; } = [];

    public event EventHandler<InstallationStageViewModel>? StageSkipped;

    public InstallationViewModel()
    {
        string enumTypeName = typeof(TStage).Name;

        foreach (var name in Enum.GetNames<TStage>())
            Stages.Add(Enum.Parse<TStage>(name), new InstallationStageViewModel
            { TaskName = LocalizedStrings.GetString($"Tasks_DownloadPage__{enumTypeName}_{name}") });
    }

    public void Report(IInstallerProgress value)
    {
        if (value is InstallerProgress<TStage> progress)
        {
            var vm = Stages[progress.Stage];
            App.DispatcherQueue.TryEnqueue(() => vm.UpdateProgress(value.StageProgress));

            if (value.StageProgress.Type == InstallerStageProgressType.Skiped)
                StageSkipped?.Invoke(this, vm);
        }
    }
}

partial class InstallationStageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string TaskName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(FontIcon))]
    public partial TaskState State { get; set; } = TaskState.Prepared;

    [ObservableProperty]
    public partial int TotalTasks { get; set; } = 1;

    public int FinishedTasks
    {
        get => _finishedTasks;
        set
        {
            _finishedTasks = value;
            OnPropertyChanged(nameof(FinishedTasks));
        }
    }
    private int _finishedTasks = 0;

    public bool IsRunning => State == TaskState.Running;

    public string FontIcon => State switch
    {
        TaskState.Cancelled => "\ue73c",
        TaskState.Finished => "\ue73e",
        TaskState.Failed => "\ue711",
        _ => "\ue73c",
    };

    public void UpdateProgress(InstallerStageProgress payload)
    {
        switch (payload.Type)
        {
            case InstallerStageProgressType.Starting:
                State = TaskState.Running;
                break;

            case InstallerStageProgressType.UpdateTotalTasks:
                TotalTasks = (int)payload.TotalTasks!;
                break;
            case InstallerStageProgressType.UpdateFinishedTasks:
                FinishedTasks = (int)payload.FinishedTasks!;
                break;
            case InstallerStageProgressType.IncrementFinishedTasks:
                Interlocked.Increment(ref _finishedTasks);
                OnPropertyChanged(nameof(FinishedTasks));
                break;

            case InstallerStageProgressType.Finished:
                State = TaskState.Finished;
                FinishedTasks = TotalTasks;
                break;
            case InstallerStageProgressType.Failed:
                State = TaskState.Failed;
                break;

            default:
                break;
        }
    }
}

internal partial class InstallInstanceTaskViewModel(
    DownloadService downloadService,
    IInstanceInstaller instanceInstaller,
    InstanceInstallConfig instanceInstallConfig,
    ObservableCollection<InstallationStageViewModel> stageViewModels) : TaskViewModel
{
    protected MinecraftInstance? _minecraftInstance;

    protected override ILogger Logger { get; } = App.GetService<ILogger<InstallInstanceTaskViewModel>>();

    #region Basic Properties

    public override string Title => instanceInstallConfig.InstanceId;

    public override string Icon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\ue896",
    };

    #endregion

    #region Stage Properties

    public ObservableCollection<InstallationStageViewModel> StageViewModels { get; } = stageViewModels;

    #endregion

    [ObservableProperty]
    public partial bool Installed { get; set; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _minecraftInstance = await instanceInstaller.InstallAsync(cancellationToken);
        InstanceConfig config = _minecraftInstance.GetConfig();

        if (instanceInstallConfig.EnableIndependencyInstance)
        {
            config.EnableIndependencyCore = true;
            config.EnableSpecialSetting = true;
        }

        App.GetService<GameService>().RefreshGames();
        await App.DispatcherQueue.EnqueueAsync(() => Installed = true);

        #region Download Mods

        string modsFolder = _minecraftInstance.GetModsDirectory();

        if (instanceInstallConfig.SecondaryLoader?.SelectedInstallData is OptiFineInstallData installData)
        {
            downloadService.DownloadResourceFileAsync(
                $"https://bmclapi2.bangbang93.com/optifine/{instanceInstallConfig.ManifestItem.Id}/{installData.Type}/{installData.Patch}",
                Path.Combine(modsFolder, installData.FileName)).Forget();
        }

        foreach (var item in instanceInstallConfig.AdditionalMods)
            downloadService.DownloadResourceFileAsync(item, modsFolder).Forget();

        #endregion
    }

    [RelayCommand]
    void Launch() => App.GetService<LaunchService>().LaunchFromUI(_minecraftInstance!);

    [RelayCommand]
    void OpenInstanceFolder() => ExplorerHelper.OpenFolder(_minecraftInstance!.GetGameDirectory());

    [RelayCommand]
    void Retry() => RetryBehavior();

    [RelayCommand]
    protected void Remove() => downloadService.DownloadTasks.Remove(this);

    protected virtual void RetryBehavior()
    {
        downloadService.InstallInstanceAsync(instanceInstallConfig).Forget();
        Remove();
    }

    #region Exception

    public override string InfoBarTitle => LocalizedStrings.Notifications__TaskFailed_Install;

    public override string ExceptionTitle
    {
        get
        {
            return ExecuteTask?.Exception?.InnerException switch
            {
                IncompleteDependenciesException => LocalizedStrings.Exceptions__IncompleteDependenciesException,
                TaskCanceledException => LocalizedStrings.Exceptions__TaskCanceledException,
                _ => string.Empty
            };
        }
    }

    protected override void NotifyException(INotificationService notificationService)
        => notificationService.InstallFailed(ExecuteTask.Exception!.InnerException!, ExceptionTitle);

    #endregion
}

#endregion

#region Install Modpack Task

internal partial class InstallModpackTaskViewModel : InstallInstanceTaskViewModel
{
    private readonly DownloadService _downloadService;
    private readonly IInstanceInstaller _instanceInstaller;

    private readonly ModpackInstallConfig _modpackInstallConfig;

    public InstallModpackTaskViewModel(
        DownloadService downloadService,
        IInstanceInstaller instanceInstaller,
        ModpackInstallConfig modpackInstallConfig,
        ObservableCollection<InstallationStageViewModel> stageViewModels)
        : base(downloadService, instanceInstaller, null!, stageViewModels)
    {
        _downloadService = downloadService;
        _instanceInstaller = instanceInstaller;

        _modpackInstallConfig = modpackInstallConfig;
    }

    protected override ILogger Logger { get; } = App.GetService<ILogger<InstallModpackTaskViewModel>>();

    public override string Title => _modpackInstallConfig.InstanceId;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _minecraftInstance = await _instanceInstaller.InstallAsync(cancellationToken);

        InstanceConfig config = _minecraftInstance.GetConfig();
        config.EnableIndependencyCore = true;
        config.EnableSpecialSetting = true;

        App.GetService<GameService>().RefreshGames();
        await App.DispatcherQueue.EnqueueAsync(() => Installed = true);
    }

    protected override void RetryBehavior()
    {
        _downloadService.InstallModpackAsync(_modpackInstallConfig).Forget();
        Remove();
    }
}

#endregion

#region Launch Task

class LaunchProgressViewModel : IProgress<LaunchProgress>
{
    public Dictionary<LaunchStage, LaunchStageViewModel> Stages { get; } = [];

    private LaunchStage _currentStage;
    public LaunchStage CurrentStage
    {
        get => _currentStage;
        set
        {
            if (_currentStage != value)
            {
                _currentStage = value;
                CurrentStageChanged?.Invoke(this, Stages[value]);
            }
        }
    }

    public event EventHandler<LaunchStageViewModel>? CurrentStageChanged;

    public LaunchProgressViewModel()
    {
        foreach (var name in Enum.GetNames<LaunchStage>())
            Stages.Add(Enum.Parse<LaunchStage>(name), new LaunchStageViewModel
            { TaskName = LocalizedStrings.GetString($"Tasks_LaunchPage__TaskName_{name}") });
    }

    public virtual void Report(LaunchProgress value)
    {
        var vm = Stages[value.Stage];

        App.DispatcherQueue.TryEnqueue(() =>
        {
            CurrentStage = value.Stage;
            vm.UpdateProgress(value.StageProgress);
        });
    }
}

class QuickLaunchProgressViewModel : LaunchProgressViewModel
{
    private uint sequence = 1;
    public const string GroupName = "Natsurainko.FluentLauncher";

    public readonly MinecraftInstance _minecraftInstance;
    public readonly Guid Guid = Guid.NewGuid();

    public AppNotification AppNotification { get; }

    public string InstanceDisplayName { get; }

    public QuickLaunchProgressViewModel(MinecraftInstance instance) : base()
    {
        _minecraftInstance = instance;
        InstanceDisplayName = instance.GetDisplayName();

        AppNotification = new AppNotificationBuilder()
            .AddArgument("guid", Guid.ToString())
            //.SetAppLogoOverride(new Uri(icon), AppNotificationImageCrop.Default)
            .AddText($"Launching Game: {InstanceDisplayName}")
            .AddText("This may take some time, please wait")
            .AddProgressBar(new AppNotificationProgressBar()
                .BindTitle()
                .BindValue()
                .BindValueStringOverride()
                .BindStatus())
            //.AddButton(new AppNotificationButton("Open Launcher")
            //    .AddArgument("action", "OpenApp"))
            .BuildNotification();

        AppNotification.Tag = Guid.ToString();
        AppNotification.Group = GroupName;
    }

    public override void Report(LaunchProgress value)
    {
        var vm = Stages[value.Stage];
        vm.UpdateProgress(value.StageProgress);

        var data = new AppNotificationProgressData(sequence)
        {
            Title = InstanceDisplayName,
            Value = vm.FinishedTasks / (double)vm.TotalTasks,
            ValueStringOverride = $"{vm.FinishedTasks} / {vm.TotalTasks}",
            Status = vm.TaskName
        };

        AppNotification.Progress = data;
        AppNotificationManager.Default.UpdateAsync(data, Guid.ToString(), GroupName)
            .GetAwaiter().GetResult();

        sequence++;

        if (value.Stage == LaunchStage.LaunchProcess && vm.FinishedTasks == vm.TotalTasks)
            _ = OnFinished();
    }

    private async Task OnFinished()
    {
        await AppNotificationManager.Default.RemoveByTagAndGroupAsync(Guid.ToString(), GroupName);
        var appNotification = new AppNotificationBuilder()
            //.SetAppLogoOverride(new Uri(icon), AppNotificationImageCrop.Default)
            .AddText($"Minecraft: {InstanceDisplayName} Launched successfully")
            .AddText("Waiting for the game window to appear")
            .BuildNotification();

        AppNotificationManager.Default.Show(appNotification);
    }
}

partial class LaunchStageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string TaskName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(FontIcon))]
    public partial TaskState State { get; set; } = TaskState.Prepared;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    public partial int TotalTasks { get; set; } = 1;

    public string ProgressPercentage => (FinishedTasks / (double)TotalTasks).ToString("P1");

    public int FinishedTasks
    {
        get => _finishedTasks;
        set
        {
            _finishedTasks = value;
            OnPropertyChanged(nameof(FinishedTasks));
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }
    private int _finishedTasks = 0;

    public bool IsRunning => State == TaskState.Running;

    public string FontIcon => State switch
    {
        TaskState.Cancelled => "\ue73c",
        TaskState.Finished => "\ue73e",
        TaskState.Failed => "\ue711",
        _ => "\ue73c",
    };

    public void UpdateProgress(LaunchStageProgress payload)
    {
        switch (payload.Type)
        {
            case LaunchStageProgressType.Starting:
                State = TaskState.Running;
                break;

            case LaunchStageProgressType.UpdateTotalTasks:
                TotalTasks = (int)payload.TotalTasks!;
                break;
            case LaunchStageProgressType.UpdateFinishedTasks:
                FinishedTasks = (int)payload.FinishedTasks!;
                break;
            case LaunchStageProgressType.IncrementFinishedTasks:
                Interlocked.Increment(ref _finishedTasks);
                OnPropertyChanged(nameof(FinishedTasks));
                OnPropertyChanged(nameof(ProgressPercentage));
                break;

            case LaunchStageProgressType.Finished:
                State = TaskState.Finished;
                FinishedTasks = TotalTasks;
                break;
            case LaunchStageProgressType.Failed:
                State = TaskState.Failed;
                break;

            default:
                break;
        }
    }
}

readonly record struct LaunchStageProgress(
    LaunchStageProgressType Type,
    int? FinishedTasks,
    int? TotalTasks)
{
    internal static LaunchStageProgress Starting()
        => new(LaunchStageProgressType.Starting, null, null);
    internal static LaunchStageProgress UpdateTotalTasks(int totalTasks)
        => new(LaunchStageProgressType.UpdateTotalTasks, null, totalTasks);
    internal static LaunchStageProgress UpdateFinishedTasks(int finishedTasks)
        => new(LaunchStageProgressType.UpdateFinishedTasks, finishedTasks, null);
    internal static LaunchStageProgress IncrementFinishedTasks()
        => new(LaunchStageProgressType.IncrementFinishedTasks, null, null);
    internal static LaunchStageProgress Finished()
        => new(LaunchStageProgressType.Finished, null, null);
    internal static LaunchStageProgress Failed()
        => new(LaunchStageProgressType.Failed, null, null);

    internal static LaunchStageProgress Skiped()
        => new(LaunchStageProgressType.Skiped, null, null);

    public enum LaunchStageProgressType
    {
        Starting,

        UpdateTotalTasks,
        UpdateFinishedTasks,
        IncrementFinishedTasks,

        Finished,
        Skiped,
        Failed,
    }
}

internal partial class LaunchTaskViewModel : TaskViewModel
{
    private readonly MinecraftInstance _instance;
    private readonly LaunchService _launchService;
    private readonly LaunchProgressViewModel launchProgressViewModel = new();

    protected override ILogger Logger { get; } = App.GetService<ILogger<LaunchTaskViewModel>>();

    public MinecraftProcess McProcess { get; private set; } = null!;

    public ObservableCollection<GameLoggerOutput> ProcessLogger { get; } = [];

    public LaunchTaskViewModel(MinecraftInstance instance, LaunchService launchService)
    {
        _instance = instance;
        _launchService = launchService;

        StageViewModels = launchProgressViewModel.Stages.Values;
        launchProgressViewModel.CurrentStageChanged += (sender, vm) =>
        {
            CurrentStage = vm;
            CurrentStageNumber++;
        };
    }

    #region Basic Properties

    public override string Title => _instance.GetDisplayName();

    public override string Icon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\ue945",
    };

    #endregion

    #region Process Properties

    private bool _isMcProcessKilled = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    public partial bool ProcessLaunched { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    public partial bool ProcessExited { get; set; }

    [ObservableProperty]
    public partial bool WaitedForInputIdle { get; set; } = false;

    #endregion

    #region Stage Properties

    public IEnumerable<LaunchStageViewModel> StageViewModels { get; }

    [ObservableProperty]
    public partial LaunchStageViewModel? CurrentStage { get; set; }

    [ObservableProperty]
    public partial int CurrentStageNumber { get; set; } = 1;

    #endregion

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    public partial bool IsLaunching { get; set; } = true;

    [ObservableProperty]
    public partial bool Crashed { get; set; } = false;

    public override bool CanCancel => IsLaunching && TaskState != TaskState.Canceling;

    public bool IsGameRunning => ProcessLaunched && !ProcessExited;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            McProcess = await _launchService.LaunchAsync(
                _instance,
                Process_OutputDataReceived,
                Process_ErrorDataReceived,
                launchProgressViewModel, cancellationToken);
            McProcess.Process.Exited += Process_Exited;
        }
        finally
        {
            Timer.Stop();
            await App.DispatcherQueue.EnqueueAsync(() => IsLaunching = false);
        }

        await App.DispatcherQueue.EnqueueAsync(() =>
        {
            Progress = 1;
            ProcessLaunched = true;
        });

        Task.Run(async () =>
        {
            McProcess.Process.WaitForInputIdle(15 * 1000);
            await App.DispatcherQueue.EnqueueAsync(() => WaitedForInputIdle = true);
        }, cancellationToken).Forget();

        await McProcess.Process.WaitForExitAsync(cancellationToken);
    }

    async void Process_Exited(object? sender, EventArgs e)
    {
        await App.DispatcherQueue.EnqueueAsync(() =>
        {
            ProcessExited = true;
            Crashed = !_isMcProcessKilled && McProcess.Process.ExitCode != 0;
            TaskState = _isMcProcessKilled
                ? TaskState.Finished
                : Crashed
                    ? TaskState.Finished
                    : TaskState.Failed;
        });

        McProcess.Dispose();
    }

    void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            ProcessLogger.Add(GameLoggerOutput.Parse(e.Data, true));
    }

    void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            ProcessLogger.Add(GameLoggerOutput.Parse(e.Data));
    }

    [RelayCommand]
    void ShowLogger()
    {
        WinUIWindowService windowService = (App.GetService<IActivationService>().ActivateWindow("LoggerWindow") as WinUIWindowService)!;
        (windowService.Window as LoggerWindow)!.Initialize(this);

        windowService.Activate();
    }

    [RelayCommand(CanExecute = nameof(IsGameRunning))]
    public void KillProcess()
    {
        _isMcProcessKilled = true;
        McProcess!.Process.KillProcessTree(); // not null when game is running
    }

    [RelayCommand]
    async Task CreateScript(IDialogActivationService<ContentDialogResult> dialogActivationService)
        => await dialogActivationService.ShowAsync("CreateLaunchScriptDialog", McProcess);

    [RelayCommand]
    void Retry()
    {
        _launchService.LaunchFromUI(_instance);
        Remove();
    }

    [RelayCommand]
    void Remove() => _launchService.LaunchTasks.Remove(this);

    [RelayCommand]
    void GoToInstanceSettings() => WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("Instances/Navigation", _instance));

    #region Exception

    public override string InfoBarTitle => LocalizedStrings.Notifications__TaskFailed_Launch;

    public override string ExceptionTitle
    {
        get
        {
            return ExecuteTask?.Exception?.InnerException switch
            {
                InstanceDirectoryNotFoundException instanceDirectoryNotFoundException => LocalizedStrings.Exceptions__InstanceDirectoryNotFoundException
                    .Replace("${instance}", instanceDirectoryNotFoundException.MinecraftInstance.InstanceId)
                    .Replace("${directory}", instanceDirectoryNotFoundException.Directory),
                JavaRuntimeFileNotFoundException javaRuntimeFileNotFoundException => LocalizedStrings.Exceptions__JavaRuntimeFileNotFoundException
                    .Replace("${file}", javaRuntimeFileNotFoundException.FileName),
                JavaRuntimeIncompatibleException javaRuntimeIncompatibleException => LocalizedStrings.Exceptions__JavaRuntimeIncompatibleException
                    .Replace("${version}", javaRuntimeIncompatibleException.TargetJavaVersion.ToString()),
                X86JavaRuntimeMemoryException => LocalizedStrings.Exceptions__X86JavaRuntimeMemoryException,
                NoActiveJavaRuntimeException => LocalizedStrings.Exceptions__NoActiveJavaRuntimeException,
                NoActiveAccountException => LocalizedStrings.Exceptions__NoActiveAccountException,
                AccountNotFoundException accountNotFoundException => LocalizedStrings.Exceptions__AccountNotFoundException
                    .Replace("${account}", accountNotFoundException.Account.Name),
                YggdrasilAuthenticationException => LocalizedStrings.Exceptions__MicrosoftAuthenticationException,
                MicrosoftAuthenticationException => LocalizedStrings.Exceptions__MicrosoftAuthenticationException,
                IncompleteDependenciesException => LocalizedStrings.Exceptions__IncompleteDependenciesException,
                //UnauthorizedAccessException unauthorizedAccessException => ,
                TaskCanceledException => LocalizedStrings.Exceptions__TaskCanceledException,
                _ => string.Empty
            };
        }
    }

    protected override void NotifyException(INotificationService notificationService)
        => notificationService.LaunchFailed(ExecuteTask.Exception!.InnerException!, ExceptionTitle);

    #endregion

    #region Timer Override

    protected override string StopwatchFormat => "mm\\:ss\\.fff";

    protected override Timer CreateTimer()
    {
        TimeSpan timerSpan = TimeSpan.FromMilliseconds(250);

        Timer timer = new(timerSpan);
        timer.Elapsed += Timer_Elapsed;

        return timer;
    }

    #endregion
}

#endregion

internal static partial class TaskViewModelNotifications
{
    [ExceptionNotification(Title = "Notifications__TaskFailed_Launch", Message = "{reason}")]
    public static partial void LaunchFailed(this INotificationService notificationService, Exception exception, string reason);

    [ExceptionNotification(Title = "Notifications__TaskFailed_Install", Message = "{reason}")]
    public static partial void InstallFailed(this INotificationService notificationService, Exception exception, string reason);

    [Notification<TeachingTip>(Title = "Notifications__DownloadUrlCopied")]
    public static partial void DownloadUrlCopied(this INotificationService notificationService);

    [Notification<TeachingTip>(Title = "Notifications__ExceptionCopied")]
    public static partial void ExceptionCopied(this INotificationService notificationService);
}

internal static partial class TaskViewModelLoggers
{
    [LoggerMessage(LogLevel.Information, "Task Enqueued")]
    public static partial void TaskEnqueued(this ILogger logger);

    [LoggerMessage(LogLevel.Error, "Task Ran To Completion")]
    public static partial void TaskRanToCompletion(this ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Task Canceld")]
    public static partial void TaskCancelld(this ILogger logger);

    [LoggerMessage(LogLevel.Error, "Task Faulted")]
    public static partial void TaskFaulted(this ILogger logger, Exception? ex);
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

public enum TaskState
{
    Prepared,
    Running,
    Finished,
    Failed,
    Cancelled,
    Canceling
}

internal abstract partial class TaskViewModel : ObservableObject
{
    protected readonly CancellationTokenSource _tokenSource = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(TaskIcon))]
    [NotifyPropertyChangedFor(nameof(ProgressShowPaused))]
    [NotifyPropertyChangedFor(nameof(ProgressShowError))]
    private TaskState taskState = TaskState.Prepared;

    [ObservableProperty]
    private string taskTitle;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    private double progress = 0;

    [ObservableProperty]
    private string timeUsage;

    [ObservableProperty]
    public bool progressBarIsIndeterminate = true;

    public bool ProgressShowError => TaskState == TaskState.Failed;

    public bool ProgressShowPaused => TaskState == TaskState.Cancelled;

    public string ProgressPercentage => Progress.ToString("P1");

    public bool CanCancel => TaskState == TaskState.Running;

    public string TaskIcon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\ue896",
    };


    public Stopwatch Stopwatch = new Stopwatch();

    public virtual void Start()
    {
        System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
        {
            App.DispatcherQueue.TryEnqueue(() => TimeUsage = Stopwatch.Elapsed.ToString("hh\\:mm\\:ss"));

            if (this.TaskState == TaskState.Failed || this.TaskState == TaskState.Finished || this.TaskState == TaskState.Cancelled)
            {
                Stopwatch.Stop();

                timer.Stop();
                timer.Dispose();
            }
        };

        Stopwatch.Start();
        Task.Run(timer.Start);
        Task.Run(Run);
    }

    protected abstract void Run();

    [RelayCommand(CanExecute = nameof(CanCancel))]
    void Cancel()
    {
        _tokenSource.Cancel();
        TaskState = TaskState.Canceling;
    }
}

#region Download Task

internal partial class DownloadGameResourceTaskViewModel : TaskViewModel
{
    private readonly string _filePath;
    private readonly GameResourceFile _resourceFile;

    public DownloadGameResourceTaskViewModel(GameResourceFile resourceFile, string filePath)
    {
        _filePath = filePath;
        _resourceFile = resourceFile;

        TaskTitle = resourceFile.FileName;
    }

    protected override async void Run()
    {
        App.DispatcherQueue.TryEnqueue(() => TaskState = TaskState.Running);

        string url = await _resourceFile.GetUrl();

        var downloadTask = HttpUtils.Downloader.CreateDownloadTask(url, _filePath);
        downloadTask.FileSizeReceived += (long? obj) =>
        {
            if (obj != null)
                App.DispatcherQueue.TryEnqueue(() => ProgressBarIsIndeterminate = false);
        };

        void TimerInvoker(object _)
        {
            if (downloadTask.TotalBytes is null)
                return;

            App.DispatcherQueue.SynchronousTryEnqueue(() => Progress =
                (double)downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
        }

        using Timer t = new(TimerInvoker, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        var downloadResult = await downloadTask.StartAsync(_tokenSource.Token);
        TimerInvoker(null);

        App.DispatcherQueue.TryEnqueue(() =>
        {
            TaskState = downloadResult.Type switch
            {
                DownloadResultType.Failed => TaskState.Failed,
                DownloadResultType.Cancelled => TaskState.Cancelled,
                DownloadResultType.Successful => TaskState.Finished,
            };
        });

        //if (downloadResult.Exception != null)
        //    App.GetService<NotificationService>().NotifyException("", downloadResult.Exception);
    }

    [RelayCommand]
    void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{_filePath}"));
    }
}

#endregion

#region Install Instance Task

class InstallationViewModel<TStage> : IProgress<InstallerProgress<TStage>>
    where TStage : notnull
{
    public Dictionary<TStage, InstallationStageViewModel> Stages { get; } = [];

    public InstallationViewModel()
    {
        foreach (var name in Enum.GetNames(typeof(TStage)))
            Stages.Add((TStage)Enum.Parse(typeof(TStage), name), new InstallationStageViewModel { TaskName = name });
    }

    public void Report(InstallerProgress<TStage> value)
    {
        var vm = Stages[value.Stage];
        App.DispatcherQueue.TryEnqueue(() => vm.UpdateProgress(value.StageProgress));
    }
}

partial class InstallationStageViewModel : ObservableObject
{
    [ObservableProperty]
    private string taskName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(FontIcon))]
    private TaskState state = TaskState.Prepared;

    [ObservableProperty]
    private int totalTasks = 1;

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

internal partial class InstallInstanceTaskViewModel : TaskViewModel
{
    private readonly IInstanceInstaller _installer;
    private readonly InstanceInstallConfig _instanceInstallConfig;

    public IEnumerable<InstallationStageViewModel> StageViewModels { get; }

    public InstallInstanceTaskViewModel(
        IInstanceInstaller instanceInstaller,
        InstanceInstallConfig instanceInstallConfig,
        IEnumerable<InstallationStageViewModel> stageViewModels)
    {
        _installer = instanceInstaller;
        _instanceInstallConfig = instanceInstallConfig;

        StageViewModels = stageViewModels;
        TaskTitle = _instanceInstallConfig.InstanceId;
        IsExpanded = true;
    }

    [ObservableProperty]
    private bool canLaunch = false;

    protected override async void Run()
    {
        App.DispatcherQueue.SynchronousTryEnqueue(() => TaskState = TaskState.Running);
        MinecraftInstance instance = null;
        TaskState resultState = TaskState.Finished;

        try
        {
            instance = await _installer.InstallAsync(_tokenSource.Token);
        }
        catch (AggregateException aggregateException)
        {
            if (aggregateException.InnerException is OperationCanceledException)
            {
                resultState = TaskState.Cancelled;
            }
        }
        catch (Exception ex)
        {
            //if (downloadResult.Exception != null)
            //    App.GetService<NotificationService>().NotifyException("", downloadResult.Exception);

            resultState = TaskState.Failed;
        }

        App.DispatcherQueue.SynchronousTryEnqueue(() =>
        {
            ProgressBarIsIndeterminate = false;
            Progress = 1;

            TaskState = resultState;
            CanLaunch = resultState == TaskState.Finished;
        });

        if (resultState == TaskState.Finished && instance != null)
            FinishTask(instance);
    }

    void FinishTask(MinecraftInstance minecraftInstance)
    {
        var downloadService = App.GetService<DownloadService>();
        var instanceConfigService = App.GetService<InstanceConfigService>();
        var gameService = App.GetService<GameService>();

        gameService.RefreshGames();

        string minecraftFolder = gameService.ActiveMinecraftFolder;
        string modsFolder = _instanceInstallConfig.EnableIndependencyInstance
            ? Path.Combine(minecraftFolder, "versions", _instanceInstallConfig.InstanceId, "mods")
            : Path.Combine(minecraftFolder, "mods");

        foreach (var item in _instanceInstallConfig.AdditionalResources)
            downloadService.DownloadResourceFile(item, Path.Combine(modsFolder, item.FileName));

        var config = instanceConfigService.GetConfig(minecraftInstance);

        if (_instanceInstallConfig.EnableIndependencyInstance)
        {
            config.EnableIndependencyCore = true;
            config.EnableSpecialSetting = true;
        }

        if (!string.IsNullOrEmpty(_instanceInstallConfig.NickName))
        {
            config.NickName = _instanceInstallConfig.NickName;
            config.EnableSpecialSetting = true;
        }
    }
}

#endregion
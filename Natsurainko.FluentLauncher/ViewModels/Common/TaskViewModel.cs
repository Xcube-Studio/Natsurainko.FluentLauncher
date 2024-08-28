using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Experimental.GameManagement.Downloader;
using Nrk.FluentCore.Experimental.GameManagement.Installer;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

internal partial class TaskViewModel : ObservableObject
{
    protected readonly CancellationTokenSource _tokenSource = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(TaskIcon))]
    private TaskState taskState = TaskState.Prepared;

    [ObservableProperty]
    private string taskTitle;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    private double progress = 0;

    public string ProgressPercentage => Progress.ToString("P1");

    public bool CanCancel => TaskState == TaskState.Running;

    public string TaskIcon => (this.TaskState == TaskState.Failed
                || this.TaskState == TaskState.Cancelled) ? "\ue711" : "\ue896";

    [RelayCommand(CanExecute = nameof(CanCancel))]
    void Cancel()
    {
        _tokenSource.Cancel();
        TaskState = TaskState.Canceling;
    }
}

#region Download Task

internal partial class DownloadGameResourceTaskViewModel
    (GameResourceFile resourceFile, string filePath) : TaskViewModel
{
    private readonly string _filePath = filePath;
    private readonly GameResourceFile _resourceFile = resourceFile;

    public void Start() => Task.Run(Run);

    async void Run()
    {
        App.DispatcherQueue.SynchronousTryEnqueue(() => TaskState = TaskState.Running);

        string url = await _resourceFile.GetUrl();
        var downloadTask = HttpUtils.Downloader.CreateDownloadTask(url, _filePath);

        using Timer t = new((_) =>
        {
            if (downloadTask.TotalBytes is null)
                return;

            App.DispatcherQueue.TryEnqueue(() => Progress = downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        var downloadResult = await downloadTask.StartAsync(_tokenSource.Token);

        App.DispatcherQueue.SynchronousTryEnqueue(() => TaskState = downloadResult.Type == DownloadResultType.Failed ? TaskState.Failed : TaskState.Finished);

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

internal class InstallInstanceTaskViewModel : TaskViewModel
{
    private readonly IInstanceInstaller _installer;

    public IEnumerable<InstallationStageViewModel> StageViewModels { get; }

    public InstallInstanceTaskViewModel(
        IInstanceInstaller instanceInstaller,
        string instanceId,
        IEnumerable<InstallationStageViewModel> stageViewModels)
    {
        _installer = instanceInstaller;
        StageViewModels = stageViewModels;

        TaskTitle = $"Install Instance: {instanceId}";
    }

    public void Start() => Task.Run(Run);

    async void Run()
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

        App.DispatcherQueue.SynchronousTryEnqueue(() => TaskState = resultState);
    }
}

#endregion
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Exceptions;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;
using static Natsurainko.FluentLauncher.Services.Launch.LaunchProgress;
using static Natsurainko.FluentLauncher.ViewModels.Common.LaunchStageProgress;

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
    public readonly Stopwatch Stopwatch = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(TaskIcon))]
    [NotifyPropertyChangedFor(nameof(ProgressShowPaused))]
    [NotifyPropertyChangedFor(nameof(ProgressShowError))]
    [NotifyPropertyChangedFor(nameof(CancelButtonVisibility))]
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
    private bool progressBarIsIndeterminate = true;

    [ObservableProperty]
    private Visibility progressBarVisibility = Visibility.Visible;

    [ObservableProperty]
    private bool showException = false;

    [ObservableProperty]
    private string exceptionReason;

    public Visibility CancelButtonVisibility => (TaskState == TaskState.Cancelled || TaskState == TaskState.Finished || TaskState == TaskState.Failed) 
        ? Visibility.Collapsed 
        : Visibility.Visible;

    public bool ProgressShowError => TaskState == TaskState.Failed;

    public bool ProgressShowPaused => TaskState == TaskState.Cancelled;

    public string ProgressPercentage => Progress.ToString("P1");

    public virtual bool CanCancel => TaskState == TaskState.Running;

    public abstract string TaskIcon { get; }

    public Exception Exception { get; protected set; }

    public virtual void Start()
    {
        System.Timers.Timer timer = new(TimeSpan.FromSeconds(1));
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

        IsExpanded = false;
        TaskTitle = resourceFile.FileName;
    }

    public override string TaskIcon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\ue896",
    };

    protected override async void Run()
    {
        App.DispatcherQueue.TryEnqueue(() => TaskState = TaskState.Running);
        Timer timer = null;

        try
        {
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

            timer = new(TimerInvoker, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

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

                if (downloadResult.Exception is not null)
                {
                    Exception = downloadResult.Exception;
                    ShowException = true;
                    ExceptionReason = downloadResult.Exception.Message;
                }
            });
        }
        catch (Exception ex)
        {
            App.DispatcherQueue.TryEnqueue(() =>
            {
                TaskState = TaskState.Failed;
                Exception = ex;

                ShowException = true;
                ExceptionReason = ex.Message;

                NotifyException();
            });
        }
        finally
        {
            timer?.Dispose();
        }
    }

    [RelayCommand]
    void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{_filePath}"));
    }

    [RelayCommand]
    void NotifyException()
    {

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

    public override string TaskIcon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\ue896",
    };

    protected override async void Run()
    {
        App.DispatcherQueue.TryEnqueue(() => TaskState = TaskState.Running);
        MinecraftInstance instance = null;
        TaskState resultState = TaskState.Finished;

        try
        {
            instance = await _installer.InstallAsync(_tokenSource.Token);
        }
        catch (AggregateException aggregateException)
        {
            if (aggregateException.InnerException is OperationCanceledException canceledException)
            {
                resultState = TaskState.Cancelled;
                Exception = canceledException;

                App.DispatcherQueue.TryEnqueue(() =>
                {
                    ShowException = true;
                    ExceptionReason = canceledException.Message;
                });
            }
        }
        catch (Exception ex)
        {
            resultState = TaskState.Failed;
            Exception = ex;

            App.DispatcherQueue.TryEnqueue(() =>
            {
                ShowException = true;
                ExceptionReason = ex.Message;
            });
        }

        App.DispatcherQueue.TryEnqueue(() =>
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

        if(_instanceInstallConfig.SecondaryLoader?.SelectedInstallData is OptiFineInstallData installData)
        {
            var gameResourceFile = new GameResourceFile(Task.FromResult($"https://bmclapi2.bangbang93.com/optifine/{_instanceInstallConfig.ManifestItem.Id}/{installData.Type}/{installData.Patch}"))
            {
                FileName = installData.FileName,
                Loaders = [ModLoaderType.OptiFine.ToString()],
                Version = _instanceInstallConfig.ManifestItem.Id
            };

            downloadService.DownloadResourceFile(gameResourceFile, Path.Combine(modsFolder, gameResourceFile.FileName));
        }

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

    [RelayCommand]
    void NotifyException()
    {

    }
}

#endregion

#region Launch Task

class LaunchProgressViewModel : IProgress<LaunchProgress>
{
    public Dictionary<LaunchStage, LaunchStageViewModel> Stages { get; } = [];

    public LaunchProgressViewModel()
    {
        foreach (var name in Enum.GetNames(typeof(LaunchStage)))
            Stages.Add((LaunchStage)Enum.Parse(typeof(LaunchStage), name), new LaunchStageViewModel 
                { TaskName = ResourceUtils.GetValue("Tasks", "LaunchPage", $"_TaskName_{name}") });
    }

    public void Report(LaunchProgress value)
    {
        var vm = Stages[value.Stage];
        App.DispatcherQueue.TryEnqueue(() => vm.UpdateProgress(value.StageProgress));
    }
}

partial class LaunchStageViewModel : ObservableObject
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
    private readonly LaunchProgressViewModel launchProgressViewModel = new();
    private bool _isMcProcessKilled = false;

    public IEnumerable<LaunchStageViewModel> StageViewModels { get; }

    public MinecraftProcess McProcess { get; private set; }

    public ObservableCollection<GameLoggerOutput> Logger { get; } = [];

    public LaunchTaskViewModel(MinecraftInstance instance)
    {
        _instance = instance;

        StageViewModels = launchProgressViewModel.Stages.Values;
        TaskTitle = _instance.InstanceId;
        IsExpanded = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool isLaunching = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    private bool processLaunched;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    private bool processExited;

    [ObservableProperty]
    private bool crashed = false;

    public override bool CanCancel => IsLaunching && TaskState != TaskState.Canceling;

    public bool IsGameRunning => ProcessLaunched && !ProcessExited;

    public override string TaskIcon => TaskState switch
    {
        TaskState.Failed => "\ue711",
        TaskState.Cancelled => "\ue711",
        TaskState.Finished => "\ue73e",
        _ => "\ue945",
    };

    protected override async void Run()
    {
        App.DispatcherQueue.TryEnqueue(() => TaskState = TaskState.Running);
        TaskState resultState = TaskState.Running;

        try
        {
            McProcess = await App.GetService<LaunchService>().LaunchAsync(
                _instance, 
                Process_OutputDataReceived,
                Process_ErrorDataReceived,
                launchProgressViewModel, _tokenSource.Token);
        }
        catch (AggregateException aggregateException)
        {
            if (aggregateException.InnerException is OperationCanceledException)
            {
                resultState = TaskState.Cancelled;
            }
        }
        catch (OperationCanceledException ex)
        {
            resultState = TaskState.Cancelled;
            Exception = ex;

            App.DispatcherQueue.TryEnqueue(() =>
            {
                ShowException = true;
                ExceptionReason = ex.Message;
            });
        }
        catch (Exception ex)
        {
            resultState = TaskState.Failed;
            Exception = ex;

            App.DispatcherQueue.TryEnqueue(() =>
            {
                ShowException = true;
                ExceptionReason = ex.Message;
            });

            NotifyException();
        }

        if (resultState == TaskState.Running)
            AfterLaunchedProcess();

        App.DispatcherQueue.TryEnqueue(() =>
        {
            ProgressBarIsIndeterminate = false;
            Progress = 1;

            IsLaunching = false;
            TaskState = resultState;
        });
    }

    void AfterLaunchedProcess()
    {
        App.DispatcherQueue.TryEnqueue(() => ProcessLaunched = true);

        McProcess.Process.Exited += Process_Exited;
    }

    void Process_Exited(object sender, EventArgs e)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            ProcessExited = true;
            Crashed = McProcess.Process.ExitCode == 0;
            TaskState = _isMcProcessKilled 
                ? TaskState.Finished
                : Crashed
                    ? TaskState.Finished 
                    : TaskState.Failed;

            McProcess.Dispose();
        });
    }

    void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            Logger.Add(GameLoggerOutput.Parse(e.Data, true));
    }

    void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            Logger.Add(GameLoggerOutput.Parse(e.Data));
    }

    [RelayCommand]
    void ShowLogger()
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            var hoverColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
            hoverColor.A = 35;

            var window = new WindowEx();

            window.Title = $"Logger - {_instance.InstanceId}";

            window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));

            window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            window.AppWindow.TitleBar.ButtonBackgroundColor = window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            window.AppWindow.TitleBar.ButtonForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
            window.AppWindow.TitleBar.ButtonHoverForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
            window.AppWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
            window.SystemBackdrop = Environment.OSVersion.Version.Build >= 22000
               ? new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
               : new DesktopAcrylicBackdrop();

            (window.MinWidth, window.MinHeight) = (525, 328);
            (window.Width, window.Height) = (525, 612);

            var view = new Views.LoggerPage();
            var viewModel = new Pages.LoggerViewModel(this, view);

            viewModel.Title = window.Title;
            view.DataContext = viewModel;

            window.Content = view;
            window.Show();
        });
    }

    [RelayCommand(CanExecute = nameof(IsGameRunning))]
    public void KillProcess()
    {
        _isMcProcessKilled = true;
        McProcess!.Kill(); // not null when game is running
    }

    [RelayCommand]
    void CopyLaunchArguments()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(string.Join("\r\n", McProcess!.ArgumentList));
        Clipboard.SetContent(dataPackage);
    }

    [RelayCommand]
    void NotifyException()
    {
        string errorDescriptionKey = string.Empty;

        if (Exception is InvalidOperationException)
        {

        }
        else if (Exception is YggdrasilAuthenticationException)
        {
            errorDescriptionKey = "_LaunchGameThrowYggdrasilAuthenticationException";
        }
        else if (Exception is MicrosoftAuthenticationException)
        {
            errorDescriptionKey = "_LaunchGameThrowMicrosoftAuthenticationException";
        }

        App.GetService<NotificationService>().NotifyException(
            "_LaunchGameThrowException",
            Exception,
            errorDescriptionKey);
    }
}

#endregion
﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Exceptions;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
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
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Natsurainko.FluentLauncher.Services.Launch.LaunchProgress;
using static Natsurainko.FluentLauncher.ViewModels.LaunchStageProgress;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels;

internal abstract partial class TaskViewModel : ObservableObject
{
    protected readonly CancellationTokenSource _tokenSource = new();
    public readonly Stopwatch Stopwatch = new();

    protected string _stopwatchElapsedFormat = "hh\\:mm\\:ss";
    protected TimeSpan _timerTimeSpan = TimeSpan.FromSeconds(1);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(TaskIcon))]
    [NotifyPropertyChangedFor(nameof(ProgressShowPaused))]
    [NotifyPropertyChangedFor(nameof(ProgressShowError))]
    [NotifyPropertyChangedFor(nameof(CancelButtonVisibility))]
    public partial TaskState TaskState { get; set; } = TaskState.Prepared;

    [ObservableProperty]
    public partial string TaskTitle { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    public partial double Progress { get; set; } = 0;

    [ObservableProperty]
    public partial string TimeUsage { get; set; }

    [ObservableProperty]
    public partial bool ProgressBarIsIndeterminate { get; set; } = true;

    [ObservableProperty]
    public partial Visibility ProgressBarVisibility { get; set; } = Visibility.Visible;

    [ObservableProperty]
    public partial bool ShowException { get; set; } = false;

    [ObservableProperty]
    public partial string ExceptionReason { get; set; }

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
        System.Timers.Timer timer = new(_timerTimeSpan);
        timer.Elapsed += (sender, e) =>
        {
            App.DispatcherQueue.TryEnqueue(() => TimeUsage = Stopwatch.Elapsed.ToString(_stopwatchElapsedFormat));

            if (this.TaskState == TaskState.Failed || this.TaskState == TaskState.Finished || this.TaskState == TaskState.Cancelled)
            {
                if (Stopwatch.IsRunning)
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
    public void Cancel()
    {
        _tokenSource.Cancel();
        TaskState = TaskState.Canceling;
    }
}

#region Download Task

internal partial class DownloadModTaskViewModel : TaskViewModel
{
    private readonly string _folder;
    private readonly string _fileName;

    private readonly Task<string> @getUrlTask;

    public DownloadModTaskViewModel(object modFile, string folder)
    {
        if (modFile is ModrinthFile modrinthFile)
        {
            _fileName = modrinthFile.FileName;
            @getUrlTask = Task.FromResult(modrinthFile.Url);
        }
        else if (modFile is CurseForgeFile curseForgeFile)
        {
            _fileName = curseForgeFile.FileName;
            @getUrlTask = App.GetService<CurseForgeClient>().GetFileUrlAsync(curseForgeFile);
        }
        else throw new InvalidDataException();

        _folder = folder;

        IsExpanded = false;
        TaskTitle = _fileName;
    }

    public DownloadModTaskViewModel(string fileName, string url, string folder)
    {
        IsExpanded = false;
        TaskTitle = fileName;

        _fileName = fileName;
        _folder = folder;
        @getUrlTask = Task.FromResult(url);
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
        await App.DispatcherQueue.EnqueueAsync(() => TaskState = TaskState.Running);
        Timer timer = null;

        try
        {
            string url = await @getUrlTask;

            var downloadTask = HttpUtils.Downloader.CreateDownloadTask(url, Path.Combine(_folder, _fileName));

            downloadTask.FileSizeReceived += length => 
                App.DispatcherQueue.TryEnqueue(() => ProgressBarIsIndeterminate = length == null);


            void TimerInvoker(object _)
            {
                double progress = downloadTask.TotalBytes is not null
                    ? (double)downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes
                    : 0;

                App.DispatcherQueue.TryEnqueue(() => Progress = progress);
            }

            timer = new(TimerInvoker, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            var downloadResult = await downloadTask.StartAsync(_tokenSource.Token);
            TimerInvoker(null);

            await App.DispatcherQueue.EnqueueAsync(() =>
            {
                TaskState = downloadResult.Type switch
                {
                    DownloadResultType.Failed => TaskState.Failed,
                    DownloadResultType.Cancelled => TaskState.Cancelled,
                    DownloadResultType.Successful => TaskState.Finished,
                    _ => throw new InvalidOperationException()
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
            await App.DispatcherQueue.EnqueueAsync(() =>
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
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{Path.Combine(_folder, _fileName)}"));
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

internal partial class InstallInstanceTaskViewModel : TaskViewModel
{
    private readonly IInstanceInstaller _installer;
    private readonly InstanceInstallConfig _instanceInstallConfig;
    private MinecraftInstance _minecraftInstance;

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
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    public partial bool CanLaunch { get; set; } = false;

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
        catch (Exception ex)
        {
            if (ex.InnerException is OperationCanceledException canceledException)
            {
                resultState = TaskState.Cancelled;
                Exception = canceledException;

                await App.DispatcherQueue.EnqueueAsync(() =>
                {
                    ShowException = true;
                    ExceptionReason = canceledException.Message;
                });
            }
            else
            {
                resultState = TaskState.Failed;
                Exception = ex;

                await App.DispatcherQueue.EnqueueAsync(() =>
                {
                    ShowException = true;
                    ExceptionReason = ex.Message;
                });
            }
        }

        await App.DispatcherQueue.EnqueueAsync(() =>
        {
            ProgressBarIsIndeterminate = false;
            Progress = 1;

            TaskState = resultState;
            CanLaunch = resultState == TaskState.Finished;
            _minecraftInstance = instance;
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
            downloadService.DownloadModFile(
                installData.FileName, 
                $"https://bmclapi2.bangbang93.com/optifine/{_instanceInstallConfig.ManifestItem.Id}/{installData.Type}/{installData.Patch}", 
                modsFolder);
        }

        foreach (var item in _instanceInstallConfig.AdditionalMods)
            downloadService.DownloadModFile(item, modsFolder);

        var config = instanceConfigService.GetConfig(minecraftInstance);

        if (_instanceInstallConfig.EnableIndependencyInstance)
        {
            config.EnableIndependencyCore = true;
            config.EnableSpecialSetting = true;
        }
    }

    [RelayCommand]
    void NotifyException()
    {
        INotificationService notificationService = App.GetService<INotificationService>();

        string reason = Exception switch
        {
            IncompleteDependenciesException => LocalizedStrings.Exceptions__IncompleteDependenciesException,
            TaskCanceledException => LocalizedStrings.Exceptions__TaskCanceledException,
            _ => string.Empty
        };

        notificationService.InstallFailed(Exception, reason);
    }

    [RelayCommand(CanExecute = nameof(CanLaunch))]
    void Launch(INavigationService navigationService) => App.GetService<LaunchService>().LaunchFromUI(_minecraftInstance);
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

    public event EventHandler<LaunchStageViewModel> CurrentStageChanged;

    public LaunchProgressViewModel()
    {
        foreach (var name in Enum.GetNames(typeof(LaunchStage)))
            Stages.Add((LaunchStage)Enum.Parse(typeof(LaunchStage), name), new LaunchStageViewModel 
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
    private bool _isMcProcessKilled = false;

    public IEnumerable<LaunchStageViewModel> StageViewModels { get; }

    public MinecraftProcess McProcess { get; private set; }

    public ObservableCollection<GameLoggerOutput> Logger { get; } = [];

    public LaunchTaskViewModel(MinecraftInstance instance, LaunchService launchService)
    {
        _instance = instance;
        _launchService = launchService;

        StageViewModels = launchProgressViewModel.Stages.Values;
        TaskTitle = _instance.GetDisplayName();
        IsExpanded = true;

        _stopwatchElapsedFormat = "mm\\:ss\\.fffff";
        _timerTimeSpan = TimeSpan.FromMilliseconds(250);

        launchProgressViewModel.CurrentStageChanged += (sender, vm) =>
        {
            CurrentStage = vm;
            CurrentStageNumber++;
        };
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    public partial bool IsLaunching { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    public partial bool ProcessLaunched { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    public partial bool ProcessExited { get; set; }

    [ObservableProperty]
    public partial bool WaitedForInputIdle { get; set; }

    [ObservableProperty]
    public partial bool Crashed { get; set; } = false;

    [ObservableProperty]
    public partial LaunchStageViewModel CurrentStage { get; set; }

    [ObservableProperty]
    public partial int CurrentStageNumber { get; set; } = 1;

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
        await App.DispatcherQueue.EnqueueAsync(() => TaskState = TaskState.Running);
        TaskState resultState = TaskState.Running;

        try
        {
            McProcess = await _launchService.LaunchAsync(
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

            await App.DispatcherQueue.EnqueueAsync(() =>
            {
                ShowException = true;
                ExceptionReason = ex.Message;
            });
        }
        catch (Exception ex)
        {
            resultState = TaskState.Failed;
            Exception = ex;

            await App.DispatcherQueue.EnqueueAsync(() =>
            {
                ShowException = true;
                ExceptionReason = ex.Message;
            });

            NotifyException();
        }
        finally
        {
            Stopwatch.Stop();
        }

        if (resultState == TaskState.Running)
            AfterLaunchedProcess();

        await App.DispatcherQueue.EnqueueAsync(() =>
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

        Task.Run(() =>
        {
            McProcess.Process.WaitForInputIdle(15 * 1000);
            App.DispatcherQueue.TryEnqueue(() => WaitedForInputIdle = true);
        });
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
        WinUIWindowService windowService = App.GetService<IActivationService>().ActivateWindow("LoggerWindow") as WinUIWindowService;
        (windowService.Window as LoggerWindow).Initialize(this);

        windowService.Activate();
    }

    [RelayCommand(CanExecute = nameof(IsGameRunning))]
    public void KillProcess()
    {
        _isMcProcessKilled = true;
        McProcess!.Process.KillProcessTree(); // not null when game is running
    }

    [RelayCommand]
    void CopyLaunchArguments()
    {
        ClipboardHepler.SetText(string.Join("\r\n", McProcess!.ArgumentList));
        App.GetService<INotificationService>().ArgumentsCopied();
    }

    [RelayCommand]
    void NotifyException()
    {
        INotificationService notificationService = App.GetService<INotificationService>();

        string reason = Exception switch
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

        notificationService.LaunchFailed(Exception, reason);
    }
}

#endregion

internal static partial class TaskViewModelNotifications
{
    [ExceptionNotification(Title = "Notifications__TaskFailed_Launch", Message = "{reason}")]
    public static partial void LaunchFailed(this INotificationService notificationService, Exception exception, string reason);

    [ExceptionNotification(Title = "Notifications__TaskFailed_Install", Message = "{reason}")]
    public static partial void InstallFailed(this INotificationService notificationService, Exception exception, string reason);

    [Notification<TeachingTip>(Title = "Notifications__ArgumentsCopied")]
    public static partial void ArgumentsCopied(this INotificationService notificationService);
}
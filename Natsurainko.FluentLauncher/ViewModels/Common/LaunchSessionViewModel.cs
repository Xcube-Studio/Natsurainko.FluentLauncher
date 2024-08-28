using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Experimental.GameManagement;
using Nrk.FluentCore.Experimental.GameManagement.Dependencies;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Management;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;
using static System.Windows.Forms.AxHost;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class LaunchSessionViewModel : ObservableObject, IProgress<LaunchProgress>
{
    private readonly CancellationTokenSource _launchCancellationTokenSource = new();
    private MinecraftProcess? _mcProcess;
    private bool _isMcProcessKilled = false;

    public MinecraftInstance MinecraftInstance { get; private set; }

    public CancellationToken LaunchCancellationToken { get => _launchCancellationTokenSource.Token; }

    public LaunchSessionViewModel(MinecraftInstance instance)
    {
        MinecraftInstance = instance;
    }

    private void McProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            _gameLoggerOutputs.Add(GameLoggerOutput.Parse(e.Data, true));
    }

    private void McProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            _gameLoggerOutputs.Add(GameLoggerOutput.Parse(e.Data));
    }

    public LaunchStepItem[] StepItems { get; } =
    [
        new LaunchStepItem { Step = LaunchSessionState.Inspecting },
        new LaunchStepItem { Step = LaunchSessionState.Authenticating },
        new LaunchStepItem { Step = LaunchSessionState.CompletingResources },
        new LaunchStepItem { Step = LaunchSessionState.BuildingArguments },
        new LaunchStepItem { Step = LaunchSessionState.LaunchingProcess }
    ];

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    private LaunchSessionState sessionState;

    [ObservableProperty]
    private string? processStartTime;

    [ObservableProperty]
    private string? processExitTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private double progress;

    [ObservableProperty]
    private Exception? exception;

    [ObservableProperty]
    private string? exceptionReason;

    public ObservableCollection<GameLoggerOutput> _gameLoggerOutputs = new();

    public string ProgressText => Progress.ToString("P1");

    public void Report(LaunchProgress progress)
    {
        if (progress.State == LaunchSessionState.Inspecting)
        {
            StepItems[0].RunState = 1;
            SessionState = LaunchSessionState.Inspecting;
        }
        else if (progress.State == LaunchSessionState.Authenticating)
        {
            StepItems[1].RunState = 1;
            StepItems[0].RunState = 2;
            StepItems[0].FinishedTaskNumber = 1;
            SessionState = LaunchSessionState.Authenticating;
        }
        else if (progress.State == LaunchSessionState.CompletingResources)
        {
            StepItems[2].RunState = 1;
            StepItems[1].RunState = 2;
            StepItems[1].FinishedTaskNumber = 1;
            SessionState = LaunchSessionState.CompletingResources;

            DependencyResolver dependencyResolver = progress.DependencyResolver!;
            dependencyResolver.DependencyDownloaded += (_, _)
                => App.DispatcherQueue.TryEnqueue(UpdateDownloadProgress);
            dependencyResolver.InvalidDependenciesDetermined += (_, invalidItems) =>
            {
                App.DispatcherQueue.TryEnqueue(() =>
                {
                    StepItems[2].TaskNumber = invalidItems.Count();
                    UpdateLaunchProgress();
                });
            };
        }
        else if (progress.State == LaunchSessionState.BuildingArguments)
        {
            StepItems[3].RunState = 1;
            StepItems[2].RunState = 2;
            SessionState = LaunchSessionState.BuildingArguments;
        }
        else if (progress.State == LaunchSessionState.LaunchingProcess)
        {
            StepItems[4].RunState = 1;
            StepItems[3].RunState = 2;
            StepItems[3].FinishedTaskNumber = 1;
            SessionState = LaunchSessionState.LaunchingProcess;

            MinecraftProcess mcProcess = progress.MinecraftProcess!;
            mcProcess.OutputDataReceived += McProcess_OutputDataReceived;
            mcProcess.ErrorDataReceived += McProcess_ErrorDataReceived;
            mcProcess.Exited += (_, e) =>
            {
                App.DispatcherQueue.TryEnqueue(() =>
                {
                    ProcessExitTime = $"[{DateTime.Now:HH:mm:ss}]";
                    if (e.ExitCode == 0) // exited normally
                    {
                        IsExpanded = false;
                        SessionState = LaunchSessionState.GameExited;
                    }
                    else if (_isMcProcessKilled)
                    {
                        SessionState = LaunchSessionState.Killed;
                    }
                    else
                    {
                        SessionState = LaunchSessionState.GameCrashed;
                    }
                });
            };
            _mcProcess = mcProcess;
        }
        else if (progress.State == LaunchSessionState.GameRunning)
        {
            StepItems[4].RunState = 2;
            SessionState = LaunchSessionState.GameRunning;

            App.DispatcherQueue.TryEnqueue(() =>
            {
                ProcessStartTime = $"[{DateTime.Now:HH:mm:ss}]";
            });
        }
        else if (progress.State == LaunchSessionState.Faulted)
        {
            StepItems[(int)SessionState - 1].RunState = -1;
            SessionState = LaunchSessionState.Faulted;

            _mcProcess?.Dispose();

            var exception = progress.Exception!;
            Exception = exception;
            ExceptionReason = exception.Message;
            ShowException();
        }

        UpdateLaunchProgress();
    }

    private void UpdateLaunchProgress()
    {
        int total = StepItems.Select(x => x.TaskNumber).Sum();
        int finished = StepItems.Select(x => x.FinishedTaskNumber).Sum();

        Progress = (double)finished / total;
    }

    private void UpdateDownloadProgress()
    {
#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        Interlocked.Increment(ref StepItems[2].finishedTaskNumber);
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        StepItems[2].OnFinishedTaskNumberUpdate();

        UpdateLaunchProgress();
    }

    private bool IsGameRunning() => SessionState == LaunchSessionState.GameRunning;

    [RelayCommand(CanExecute = nameof(IsGameRunning))]
    public void KillProcess()
    {
        _isMcProcessKilled = true;
        _mcProcess!.Kill(); // not null when game is running
    }

    [RelayCommand]
    public void ShowLogger()
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            var hoverColor = App.Current.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
            hoverColor.A = 35;

            var window = new WindowEx();

            window.Title = $"Logger - {MinecraftInstance.InstanceId}";

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
    public void CopyLaunchArguments()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(string.Join("\r\n", _mcProcess!.ArgumentList));
        Clipboard.SetContent(dataPackage);
    }

    [RelayCommand]
    public void ShowException()
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

    public partial class LaunchStepItem : ObservableObject
    {
        public LaunchSessionState Step { get; set; }

        [ObservableProperty]
        private int runState = 0; // 0 未开始, 1 进行中, 2 完成, -1 失败

        [ObservableProperty]
        private int taskNumber = 1;

        [ObservableProperty]
        public int finishedTaskNumber = 0;

        internal void OnFinishedTaskNumberUpdate() => OnPropertyChanged(nameof(FinishedTaskNumber));
    }
}

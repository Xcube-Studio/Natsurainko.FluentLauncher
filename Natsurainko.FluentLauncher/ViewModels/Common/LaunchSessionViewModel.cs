using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Experimental.GameManagement;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Experimental.GameManagement.Launch;
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

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class LaunchSessionViewModel : ObservableObject, IProgress<LaunchProgress>
{
    private readonly CancellationTokenSource _launchCancellationTokenSource = new();
    private MinecraftProcess? _mcProcess;

    public MinecraftInstance MinecraftInstance { get; private set; }

    public CancellationToken LaunchCancellationToken { get => _launchCancellationTokenSource.Token; }

    public LaunchSessionViewModel(MinecraftInstance instance)
    {
        MinecraftInstance = instance;
        //_mcProcess = mcProcess;

        //// Handles all state changes
        //session.StateChanged += (_, e) => App.DispatcherQueue.TryEnqueue
        //    (() => LaunchSessionStateChangedHandler(e));

        //// Retrieve the process output streams
        //session.OutputDataReceived += McProcess_OutputDataReceived;
        //session.ErrorDataReceived += McProcess_ErrorDataReceived;

        //session.SingleFileDownloaded += (_, _) => App.DispatcherQueue.TryEnqueue(UpdateDownloadProgress);
        //session.DownloadElementsPosted += (_, count) => App.DispatcherQueue.TryEnqueue(() =>
        //{
        //    StepItems[2].TaskNumber = count;
        //    UpdateLaunchProgress();
        //});

        // TODO: show exception when falted

        //State = MinecraftSessionState.Faulted;
        //_mcProcess?.Dispose();
        //App.DispatcherQueue.TryEnqueue(() =>
        //{
        //    Exception = ex;
        //    ExceptionReason = Exception.Message;
        //});
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
        new LaunchStepItem { Step = MinecraftSessionState.Inspecting },
        new LaunchStepItem { Step = MinecraftSessionState.Authenticating },
        new LaunchStepItem { Step = MinecraftSessionState.CompletingResources },
        new LaunchStepItem { Step = MinecraftSessionState.BuildingArguments },
        new LaunchStepItem { Step = MinecraftSessionState.LaunchingProcess }
    ];

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
    private MinecraftSessionState sessionState;

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

    private void LaunchSessionStateChangedHandler(MinecraftSessionStateChagnedEventArgs e)
    {
        SessionState = e.NewState;

        int state = (int)SessionState;
        int lastState = (int)e.OldState;
        MinecraftSessionState newValue = e.NewState;

        if (2 <= state && state <= 6)
        {
            StepItems[lastState - 1].RunState = 2;
            if (state != 4) StepItems[lastState - 1].FinishedTaskNumber = 1;
        }

        if (1 <= state && state <= 5)
            StepItems[state - 1].RunState = 1;

        if ((MinecraftSessionState)lastState == MinecraftSessionState.LaunchingProcess && newValue == MinecraftSessionState.GameRunning) // Update start time only for Process.Start()
            ProcessStartTime = $"[{DateTime.Now:HH:mm:ss}]";

        // Fault before the game is started
        if (newValue == MinecraftSessionState.Faulted)
        {
            StepItems[lastState - 1].RunState = -1;
        }

        // Session finished (exited, crashed, or killed)
        if (newValue == MinecraftSessionState.GameExited)
            IsExpanded = false;

        if (newValue == MinecraftSessionState.GameExited || newValue == MinecraftSessionState.GameCrashed || newValue == MinecraftSessionState.Killed)
            ProcessExitTime = $"[{DateTime.Now:HH:mm:ss}]";

        UpdateLaunchProgress();
    }

    internal void UpdateLaunchProgress()
    {
        int total = StepItems.Select(x => x.TaskNumber).Sum();
        int finished = StepItems.Select(x => x.FinishedTaskNumber).Sum();

        Progress = (double)finished / total;
    }

    internal void UpdateDownloadProgress()
    {
#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        Interlocked.Increment(ref StepItems[2].finishedTaskNumber);
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        StepItems[2].OnFinishedTaskNumberUpdate();

        UpdateLaunchProgress();
    }

    bool CanKillProcess() => SessionState == MinecraftSessionState.GameRunning;

    public void OnExceptionThrow(Exception exception)
    {
        Exception = exception;
        ExceptionReason = exception.Message;

        ShowException();
    }

    [RelayCommand(CanExecute = nameof(CanKillProcess))]
    public void KillProcess() => _mcProcess!.Kill(); // not null when game is running

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

    [RelayCommand]
    public void CopyLaunchArguments()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(string.Join("\r\n", _mcProcess.ArgumentList));
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

    public void Report(LaunchProgress progress)
    {
        if (progress.State == LaunchSessionState.Faulted)
        {
            OnExceptionThrow(progress.Exception!);
        }
    }

    public partial class LaunchStepItem : ObservableObject
    {
        public MinecraftSessionState Step { get; set; }

        [ObservableProperty]
        private int runState = 0; // 0 未开始, 1 进行中, 2 完成, -1 失败

        [ObservableProperty]
        private int taskNumber = 1;

        [ObservableProperty]
        public int finishedTaskNumber = 0;

        internal void OnFinishedTaskNumberUpdate() => OnPropertyChanged(nameof(FinishedTaskNumber));
    }
}

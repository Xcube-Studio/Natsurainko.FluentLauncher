using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Components.Launch;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Components.Launch;

[ObservableObject]
internal partial class LaunchProcess : BaseLaunchProcess
{
    private bool isKilled;
    private Exception Exception;

    private readonly LaunchService _launchService;

    public event EventHandler GameProcessStart;

    public override LaunchState State
    {
        get => DisplayState;
        protected set => App.DispatcherQueue.SynchronousTryEnqueue(() => DisplayState = value);
    }

    public LaunchProcess(GameInfo gameInfo) : base()
    {
        GameInfo = gameInfo;
        _launchService = App.GetService<LaunchService>();

        this.PropertyChanged += LaunchProcess_PropertyChanged;
    }

    public override void KillProcess()
    {
        McProcess.Kill();
        isKilled = true;
    }

    public override void RunLaunch()
    {
        try
        {
            State = LaunchState.Inspecting;

            if (!InspectAction())
            {
                // TODO: 添加检查失败提示

                State = LaunchState.Faulted;
                return;
            }

            State = LaunchState.Authenticating;
            AuthenticateFunc();

            State = LaunchState.CompletingResources;
            CompleteResourcesAction();

            State = LaunchState.BuildingArguments;
            var arguments = BuildArgumentsFunc().ToList();

            State = LaunchState.LaunchingProcess;
            McProcess = CreateProcessFunc();

            McProcess.StartInfo.Arguments = string.Join(' ', arguments);
            McProcess.StartInfo.UseShellExecute = false;
            McProcess.StartInfo.RedirectStandardOutput = true;
            McProcess.StartInfo.RedirectStandardError = true;
            McProcess.EnableRaisingEvents = true;

            McProcess.OutputDataReceived += McProcess_OutputDataReceived;
            McProcess.ErrorDataReceived += McProcess_ErrorDataReceived;
            McProcess.Exited += McProcess_Exited;

            McProcess.Start();
            GameProcessStart.Invoke(this, null);

            State = LaunchState.GameRunning;

            McProcess.BeginOutputReadLine();
            McProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            State = LaunchState.Faulted;
            McProcess?.Dispose();
            Exception = ex.InnerException;

            throw;
        }
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

    private void McProcess_Exited(object sender, EventArgs e)
    {
        if (isKilled)
        {
            State = LaunchState.Killed;
            return;
        }

        State = McProcess.ExitCode.Equals(0) ? LaunchState.GameExited : LaunchState.GameCrashed;
    }

    #region UI Logic

    public LaunchExpanderStepItem[] StepItems { get; } = new[]
    {
        new LaunchExpanderStepItem { Step = LaunchState.Inspecting },
        new LaunchExpanderStepItem { Step = LaunchState.Authenticating },
        new LaunchExpanderStepItem { Step = LaunchState.CompletingResources },
        new LaunchExpanderStepItem { Step = LaunchState.BuildingArguments },
        new LaunchExpanderStepItem { Step = LaunchState.LaunchingProcess }
    };

    private int lastState;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    private LaunchState displayState;

    [ObservableProperty]
    private string processStartTime;

    [ObservableProperty]
    private string processExitTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private double progress;

    [ObservableProperty]
    private string exceptionReason;

    public ObservableCollection<GameLoggerOutput> _gameLoggerOutputs = new();

    public string ProgressText => Progress.ToString("P1");

    private void LaunchProcess_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DisplayState))
            HandleStateChanged();
    }

    private void HandleStateChanged()
    {
        var state = (int)DisplayState;

        if (2 <= state && state <= 6)
        {
            StepItems[lastState - 1].RunState = 2;
            if (state != 4) StepItems[lastState - 1].FinishedTaskNumber = 1;
        }

        if (1 <= state && state <= 5)
            StepItems[state - 1].RunState = 1;

        if (DisplayState == LaunchState.Faulted)
            StepItems[lastState - 1].RunState = -1;

        if (DisplayState == LaunchState.GameRunning)
            ProcessStartTime = $"[{McProcess.StartTime:HH:mm:ss}]";

        if (DisplayState == LaunchState.GameExited)
            IsExpanded = false;

        if (DisplayState == LaunchState.GameExited || DisplayState == LaunchState.GameCrashed)
            ProcessExitTime = $"[{McProcess.ExitTime:HH:mm:ss}]";

        lastState = state;

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

    [RelayCommand]
    public void KillButton() => KillProcess();

    [RelayCommand]
    public void LoggerButton()
    {
        var window = new WindowEx();

        window.Title = $"Logger - {GameInfo.AbsoluteId}";

        window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
        window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        window.SystemBackdrop = Environment.OSVersion.Version.Build >= 22000
           ? new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
           : new DesktopAcrylicBackdrop();

        (window.MinWidth, window.MinHeight) = (516, 328);
        (window.Width, window.Height) = (873, 612);

        var view = new Views.LoggerPage();
        var viewModel = new ViewModels.Pages.LoggerViewModel(this, view);
        viewModel.Title = window.Title;
        view.DataContext = viewModel;

        window.Content = view;
        window.Show();
    }

    #endregion
}

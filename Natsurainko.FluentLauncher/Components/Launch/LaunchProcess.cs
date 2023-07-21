using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Components.Launch;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components.Launch;

[ObservableObject]
internal partial class LaunchProcess : BaseLaunchProcess
{
    private bool isKilled;

    private readonly LaunchService _launchService;
    internal string _suitableJava;

    public override LaunchState State 
    { 
        get => DisplayState;
        protected set => App.MainWindow.DispatcherQueue.SynchronousTryEnqueue(() => DisplayState = value);
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

            State = LaunchState.GameRunning;

            McProcess.BeginOutputReadLine();
            McProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            State = LaunchState.Faulted;
            McProcess?.Dispose();

            throw;
        }

    }

    private void McProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        // TODO: 接受游戏输出
    }

    private void McProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        // TODO: 接受游戏输出
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
    private LaunchState displayState;
    [ObservableProperty]
    private string processStartTime;
    [ObservableProperty]
    private string processExitTime;
    [ObservableProperty]
    private double progress;

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

        if (DisplayState == LaunchState.GameExited || DisplayState == LaunchState.GameCrashed) 
            ProcessExitTime = $"[{McProcess.ExitTime:HH:mm:ss}]";

        lastState = state;

        UpdateLaunchProgress();
    }

    internal void UpdateLaunchProgress()
    {
        int total = StepItems.Select(x => x.TaskNumber).Sum();
        int finished = StepItems.Select(x => x.FinishedTaskNumber).Sum();

        Progress = (double)total / finished;
    }

    #endregion
}

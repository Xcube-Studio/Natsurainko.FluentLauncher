using CommunityToolkit.Mvvm.ComponentModel;
using Nrk.FluentCore.Launch;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class LaunchExpanderStepItem : ObservableObject
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

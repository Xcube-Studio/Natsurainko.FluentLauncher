using CommunityToolkit.Mvvm.ComponentModel;
using Nrk.FluentCore.Classes.Enums;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class LaunchExpanderStepItem : ObservableObject
{
    public LaunchState Step { get; set; }

    [ObservableProperty]
    private int runState = 0; // 0 未开始, 1 进行中, 2 完成, -1 失败

    [ObservableProperty]
    private int taskNumber = 1;

    [ObservableProperty]
    private int finishedTaskNumber = 0;
}

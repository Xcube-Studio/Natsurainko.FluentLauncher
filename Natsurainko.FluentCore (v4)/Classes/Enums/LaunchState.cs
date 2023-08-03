namespace Nrk.FluentCore.Classes.Enums;

/// <summary>
/// 启动状态
/// </summary>
public enum LaunchState
{
    Created = 0,
    Inspecting = 1,
    Authenticating = 2,
    CompletingResources = 3,
    BuildingArguments = 4,
    LaunchingProcess = 5,
    GameRunning = 6,
    GameExited = 7,
    Faulted = 8,
    Killed = 9,
    GameCrashed = 10
}

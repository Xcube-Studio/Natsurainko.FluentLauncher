using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Enums;

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

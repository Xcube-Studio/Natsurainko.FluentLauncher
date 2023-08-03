using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nrk.FluentCore.Components.Launch;

public abstract class BaseLaunchProcess
{
    public Func<bool> InspectAction { get; set; }
    public Func<IEnumerable<string>> BuildArgumentsFunc { get; set; }
    public Action CompleteResourcesAction { get; set; }
    public Action AuthenticateFunc { get; set; }
    public Func<Process> CreateProcessFunc { get; set; }

    public Process McProcess { get; protected set; }

    public GameInfo GameInfo { get; protected set; }

    public IEnumerable<string> LaunchArguments { get; protected set; }

    public virtual LaunchState State { get; protected set; }

    public abstract void RunLaunch();

    public abstract void KillProcess();
}

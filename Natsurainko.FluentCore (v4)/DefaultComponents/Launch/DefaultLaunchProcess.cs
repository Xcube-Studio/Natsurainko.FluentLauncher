using Nrk.FluentCore.Components.Launch;
using System;

namespace Nrk.FluentCore.DefaultComponents.Launch;

public class DefaultLaunchProcess : BaseLaunchProcess
{
    public DefaultLaunchProcess() : base()
    {

    }

    public override void KillProcess()
    {
        throw new NotImplementedException();
    }

    public override void RunLaunch()
    {
        if (!InspectAction())
        {
            State = Classes.Enums.LaunchState.Faulted;
            return;
        }
    }
}

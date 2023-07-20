using Nrk.FluentCore.Components.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.DefaultComponets.Launch;

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

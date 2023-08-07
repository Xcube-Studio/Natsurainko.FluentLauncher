using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Components.Launch;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nrk.FluentCore.DefaultComponents.Launch;

public class DefaultLaunchProcessBuilder : BaseLaunchProcessBuilder<DefaultLaunchProcess, DefaultLaunchProcessBuilder>
{
    public DefaultLaunchProcessBuilder(GameInfo gameInfo) : base(gameInfo)
    {
        _launchProcess = new DefaultLaunchProcess();
    }

    public override DefaultLaunchProcess Build() => _launchProcess;

    public override DefaultLaunchProcessBuilder SetAuthenticateFunc(Action authenticateFunc)
    {
        _launchProcess.AuthenticateFunc = authenticateFunc;
        return this;
    }

    public override DefaultLaunchProcessBuilder SetBuildArgumentsFunc(Func<IEnumerable<string>> buildArgumentsFunc)
    {
        _launchProcess.BuildArgumentsFunc = buildArgumentsFunc;
        return this;
    }

    public override DefaultLaunchProcessBuilder SetCompleteResourcesAction(Action completeResourcesAction)
    {
        _launchProcess.CompleteResourcesAction = completeResourcesAction;
        return this;
    }

    public override DefaultLaunchProcessBuilder SetCreateProcessFunc(Func<Process> createProcessFunc)
    {
        _launchProcess.CreateProcessFunc = createProcessFunc;
        return this;
    }

    public override DefaultLaunchProcessBuilder SetInspectAction(Func<bool> inspectAction)
    {
        _launchProcess.InspectAction = inspectAction;
        return this;
    }
}

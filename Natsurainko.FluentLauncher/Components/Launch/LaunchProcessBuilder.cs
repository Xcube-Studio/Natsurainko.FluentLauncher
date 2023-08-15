using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Components.Launch;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Natsurainko.FluentLauncher.Components.Launch;

internal class LaunchProcessBuilder : BaseLaunchProcessBuilder<LaunchProcess, LaunchProcessBuilder>
{
    public LaunchProcessBuilder(GameInfo gameInfo) : base(gameInfo)
    {
        _launchProcess = new LaunchProcess(gameInfo);
    }

    public override LaunchProcess Build() => _launchProcess;

    public override LaunchProcessBuilder SetAuthenticateFunc(Action authenticateFunc)
    {
        _launchProcess.AuthenticateFunc = authenticateFunc;
        return this;
    }

    public override LaunchProcessBuilder SetBuildArgumentsFunc(Func<IEnumerable<string>> buildArgumentsFunc)
    {
        _launchProcess.BuildArgumentsFunc = buildArgumentsFunc;
        return this;
    }

    public override LaunchProcessBuilder SetCompleteResourcesAction(Action completeResourcesAction)
    {
        _launchProcess.CompleteResourcesAction = completeResourcesAction;
        return this;
    }

    public LaunchProcessBuilder SetCompleteResourcesAction(Action<LaunchProcess> completeResourcesAction)
        => SetCompleteResourcesAction(() => completeResourcesAction(_launchProcess));

    public override LaunchProcessBuilder SetInspectAction(Func<bool> inspectAction)
    {
        _launchProcess.InspectAction = inspectAction;
        return this;
    }

    public override LaunchProcessBuilder SetCreateProcessFunc(Func<Process> createProcessFunc)
    {
        _launchProcess.CreateProcessFunc = createProcessFunc;
        return this;
    }
}

using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Components.Launch;

public abstract class BaseLaunchProcessBuilder<TProcess, TBuilder> where TProcess : BaseLaunchProcess
{
    protected TProcess _launchProcess;
    protected readonly GameInfo _gameInfo;

    public BaseLaunchProcessBuilder(GameInfo gameInfo)
    {
        _gameInfo = gameInfo; 
    }

    public abstract TBuilder SetInspectAction(Func<bool> inspectAction);

    public abstract TBuilder SetBuildArgumentsFunc(Func<IEnumerable<string>> buildArgumentsFunc);

    public abstract TBuilder SetCompleteResourcesAction(Action downloadResourcesAction);

    public abstract TBuilder SetAuthenticateFunc(Action authenticateFunc);

    public abstract TBuilder SetCreateProcessFunc(Func<Process> createProcessFunc);

    public abstract TProcess Build();
}

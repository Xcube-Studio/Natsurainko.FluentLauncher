using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinRT;

namespace FluentLauncher.Infra.WinUI.AppHost;

public class WinUIApplication : IHost
{
    public IServiceProvider Services => Host.Services;

    private readonly Func<Application> _createApplicationFunc;

    public IHost Host { get; init; }

    public static WinUIApplicationBuilder CreateBuilder(Func<Application> createApplicationFunc)
    {
        return new WinUIApplicationBuilder(createApplicationFunc);
    }

    internal WinUIApplication(Func<Application> createApplicationFunc, IHost host)
    {
        _createApplicationFunc = createApplicationFunc;
        Host = host;
    }

    public void Dispose()
    {
        return;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        [DllImport("Microsoft.ui.xaml.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        static extern void XamlCheckProcessRequirements();

        XamlCheckProcessRequirements();
        ComWrappersSupport.InitializeComWrappers();
        Application.Start(delegate
        {
            DispatcherQueueSynchronizationContext synchronizationContext = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            _createApplicationFunc();
        });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.DependencyInjection;
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

    // Task that models the execution of the Microsoft.UI.Xaml.Application.Start method
    // Completes when the MUX Application exits.
    private readonly TaskCompletionSource _winUIStartedTcs = new();

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

        var hostAppLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        Task.Run(() =>
        {
            try
            {
                XamlCheckProcessRequirements();
                ComWrappersSupport.InitializeComWrappers();
                Application.Start(delegate
                {
                    try
                    {
                        DispatcherQueueSynchronizationContext synchronizationContext = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                        _createApplicationFunc();
                        _winUIStartedTcs.SetResult(); // Signal that WinUI has started successfully
                    }
                    catch (Exception ex)
                    {
                        _winUIStartedTcs.SetException(ex); // Signal the exception if initialization fails
                    }
                });
                hostAppLifetime.StopApplication(); // WinUI app exits normally
            }
            catch (Exception ex)
            {
                _winUIStartedTcs.SetException(ex); // Signal the exception if initialization fails
            }
        }, cancellationToken);

        return _winUIStartedTcs.Task;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
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

    public void Dispose() => Host.Dispose();

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        [DllImport("Microsoft.ui.xaml.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        static extern void XamlCheckProcessRequirements();

        var hostAppLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        // Task that models the execution of the Microsoft.UI.Xaml.Application.Start method
        // Completes when the MUX Application exits.
        var winUIStartedTcs = new TaskCompletionSource();

        void RunWinUIApp()
        {
            try
            {
                XamlCheckProcessRequirements();
                ComWrappersSupport.InitializeComWrappers();
                Application.Start(delegate
                {
                    try
                    {
                        Dispatching.DispatcherQueueSynchronizationContext synchronizationContext = new(DispatcherQueue.GetForCurrentThread());
                        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                        _createApplicationFunc();
                        winUIStartedTcs.SetResult(); // Signal that WinUI has started successfully
                    }
                    catch (Exception ex)
                    {
                        winUIStartedTcs.SetException(ex); // Signal that an exception is thrown during initialization
                    }
                });
                hostAppLifetime.StopApplication(); // WinUI app exits normally
            }
            catch (Exception ex)
            {
                winUIStartedTcs.SetException(ex); // Signal that an exception is thrown during initialization
            }
        }

        // Start the WinUI app on a new STA thread
        Thread winuiThread = new(RunWinUIApp);
        winuiThread.SetApartmentState(ApartmentState.STA);
        winuiThread.Start();

        Host.RunAsync(cancellationToken); // Run the host application lifetime

        return winUIStartedTcs.Task;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

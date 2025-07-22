using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WinRT;

namespace FluentLauncher.Infra.WinUI.AppHost;

public partial class WinUIApplication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApplication> : IHost 
    where TApplication : Application
{
    public IServiceProvider Services => Host.Services;

    public IHost Host { get; init; }

    public static WinUIApplicationBuilder<TApplication> CreateBuilder() => new();

    internal WinUIApplication(IHost host)
    {
        Host = host;
    }

    public void Dispose() => Host.Dispose();

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        var hostAppLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        // Task that models the execution of the Microsoft.UI.Xaml.Application.Start method
        // Completes when the MUX Application exits.
        var winUIStartedTcs = new TaskCompletionSource();

        void RunWinUIApp()
        {
            try
            {
                StaticMethods.XamlCheckProcessRequirements();
                ComWrappersSupport.InitializeComWrappers();
                Application.Start(delegate
                {
                    try
                    {
                        Dispatching.DispatcherQueueSynchronizationContext synchronizationContext = new(DispatcherQueue.GetForCurrentThread());
                        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                        Services.GetRequiredService<TApplication>();
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

file static class StaticMethods
{
    [DllImport("Microsoft.ui.xaml.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern void XamlCheckProcessRequirements();
}
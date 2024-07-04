using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher;
using System;
using WinRT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration((config) =>
    {
        //config.AddJsonFile("appsettings.json", optional: true);
        //config.AddCommandLine(args);
    })
    .ConfigureServices((services) =>
    {

    })
    .ConfigureLogging(logging =>
    {

    })
    .Build();

XamlCheckProcessRequirements();
ComWrappersSupport.InitializeComWrappers();
Application.Start(delegate
{
    DispatcherQueueSynchronizationContext synchronizationContext = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
    SynchronizationContext.SetSynchronizationContext(synchronizationContext);
    new App();
});

await host.RunAsync();


[DllImport("Microsoft.ui.xaml.dll")]
[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
static extern void XamlCheckProcessRequirements();
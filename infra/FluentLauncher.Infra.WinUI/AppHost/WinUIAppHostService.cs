//using System.Runtime.InteropServices;
//using Microsoft.Extensions.Hosting;
//using Microsoft.UI.Dispatching;
//using Microsoft.UI.Xaml;
//using System.Threading;
//using System.Threading.Tasks;
//using WinRT;

//namespace FluentLauncher.Infra.WinUI.AppHost;

//public class WinUIHostService : IHostedService
//{
//    private TaskCompletionSource<object?> _winUIStartedTcs = new();

//    [DllImport("Microsoft.ui.xaml.dll")]
//    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
//    static extern void XamlCheckProcessRequirements();

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        Task.Run(() =>
//        {
//            XamlCheckProcessRequirements();
//            ComWrappersSupport.InitializeComWrappers();
//            Application.Start(delegate
//            {
//                DispatcherQueueSynchronizationContext synchronizationContext = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
//                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
//                _createApplicationFunc();
//                _winUIStartedTcs.SetResult(null); // Signal that WinUI has started
//            });
//        });

//        return _winUIStartedTcs.Task; // Return the task that completes when WinUI starts
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        // Implement any cleanup or shutdown logic for the WinUI application here
//        return Task.CompletedTask;
//    }
//}

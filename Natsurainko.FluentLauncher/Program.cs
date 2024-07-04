using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher;
using System;
using WinRT;

XamlCheckProcessRequirements();
ComWrappersSupport.InitializeComWrappers();
Application.Start(delegate
{
    DispatcherQueueSynchronizationContext synchronizationContext = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
    SynchronizationContext.SetSynchronizationContext(synchronizationContext);
    new App();
});

[DllImport("Microsoft.ui.xaml.dll")]
[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
static extern void XamlCheckProcessRequirements();
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Natsurainko.FluentLauncher.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher;

public partial class App
{
#if DEBUG
    [System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
    private static extern void XamlCheckProcessRequirements();
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler", " 1.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
#endif

    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length != 0)
            return WorkingProcessEntryPoint.Main(args);

#if DEBUG
        XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();
#endif
        Microsoft.UI.Xaml.Application.Start((p) =>
        {   
            var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });

        return 0;
    }
}

public partial class App : Application
{
    public static Configuration Configuration { get; private set; } = Configuration.Load();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private static MainWindow m_window;

    public static MainWindow MainWindow => m_window;
}

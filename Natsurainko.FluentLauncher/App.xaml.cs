using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.CrossProcess;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Storage;
using Windows.UI.Popups;

namespace Natsurainko.FluentLauncher;

public partial class App
{
    [DllImport("Microsoft.UI.Xaml.dll")]
    private static extern void XamlCheckProcessRequirements();

    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length != 0)
            return WorkingProcessEntryPoint.Main(args);

        XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();

        Microsoft.UI.Xaml.Application.Start((p) =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
            new App();
        });

        return 0;
    }
}

public partial class App : Application
{
    public static Configuration Configuration { get; private set; } = Configuration.Load();

    public static string StoragePath => ApplicationData.Current.LocalFolder.Path;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    public static MainWindow MainWindow { get; private set; }

    #region 尝试错误收集，但实际没有任何效果
    /*
    UnhandledException += App_UnhandledException;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        App.MainWindow.DispatcherQueue?.TryEnqueue(async () =>
        {
            var stringBuilder = new StringBuilder()
                .AppendLine(e.ExceptionObject.GetType().FullName)
                .AppendLine(e.ExceptionObject.ToString());

            var dialog = new MessageDialog(stringBuilder.ToString(), "程序遇到不可恢复的问题，请向开发者提供此窗口截图\r\n" +
                "The program has encountered an unrecoverable problem, please provide the developer with a screenshot of this window");

            dialog.Commands.Add(new UICommand("退出 Exit", (ui) => App.Current.Exit()));
            await dialog.ShowAsync();
        });
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        App.MainWindow.DispatcherQueue?.TryEnqueue(async () =>
        {
            var stringBuilder = new StringBuilder()
                .AppendLine(e.Exception.GetType().FullName)
                .AppendLine(e.Exception.ToString());

            if (e.Exception.InnerException != null)
            {
                stringBuilder.AppendLine("InnerException:");
                stringBuilder.AppendLine(e.Exception.InnerException.ToString());
            }

            var dialog = new MessageDialog(stringBuilder.ToString(), "程序遇到不可恢复的问题，请向开发者提供此窗口截图\r\n" +
                "The program has encountered an unrecoverable problem, please provide the developer with a screenshot of this window");

            dialog.Commands.Add(new UICommand("退出 Exit", (ui) => App.Current.Exit()));
            await dialog.ShowAsync();
        });
    }
    */

    #endregion
}

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.CrossProcess;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Storage;
using Windows.UI.Popups;
using System.Windows.Input;

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
        
        // Global exception handlers
        UnhandledException += App_UnhandledException;
        //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    public static MainWindow MainWindow { get; private set; }


    #region Global exception handlers

    private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        // 这里不能设置e.Handled，MS Learn文档表示只能用于记录日志后退出，或许可以尝试专门在新的进程上开一个窗口提示？
        // https://learn.microsoft.com/en-us/dotnet/api/system.appdomain?view=net-7.

        if (!e.IsTerminating && e.ExceptionObject is Exception ex)
        {
            ProcessException(ex);
        }
    }

    private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        ProcessException(e.Exception);
    }

    public static string GetErrorMessage(Exception e)
    {
        if (e is null) return string.Empty;

        var stringBuilder = new StringBuilder()
            .AppendLine(e.GetType().FullName)
            .AppendLine(e.ToString());

        if (e.InnerException != null)
        {
            stringBuilder.AppendLine("InnerException:");
            stringBuilder.AppendLine(e.InnerException.ToString());
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Show error message in an appropriate way
    /// </summary>
    /// <param name="errorMessage"></param>
    public static void ShowErrorMessage(string errorMessage)
    {
        errorMessage = errorMessage + errorMessage + errorMessage;

        if (App.MainWindow is not null)
        {
            App.MainWindow.DispatcherQueue?.TryEnqueue(async () =>
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = MainWindow.Content.XamlRoot,
                    Title = "程序运行出现问题\nThe program has encountered a problem",
                    Content = new ScrollViewer() 
                    { 
                        Content = new TextBlock { Text = errorMessage, IsTextSelectionEnabled = true },
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                    },
                    PrimaryButtonText = "关闭 Close"
                };
                await dialog.ShowAsync();
            });
        }
        else
        {
            // Error in initializing MainWindow: display the exception in a deciated new window
            var window = new Window();
            window.Content = new TextBlock() { Text = errorMessage };
            window.Activate();
        }
    }

    public static void ProcessException(Exception ex)
    {
        var errorMessage = GetErrorMessage(ex);
        // TODO: Log the error message

        ShowErrorMessage(errorMessage);
    }

    #endregion
}

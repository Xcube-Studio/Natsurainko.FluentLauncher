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
using Natsurainko.FluentLauncher.Views.Dialogs;

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
        
        // Global exception handler
        UnhandledException += (_, e) => 
        {
            e.Handled = true;
            ProcessException(e.Exception);
        };
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    public static MainWindow MainWindow { get; private set; }

    #region Global exception handlers

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
                var dialog = new ExceptionDialog(errorMessage) { XamlRoot = MainWindow.Content.XamlRoot };
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

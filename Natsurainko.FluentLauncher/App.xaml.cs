using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.SystemServices;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Activities;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using System;
using System.Text;
using System.Threading;

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    public static IServiceProvider Services => Program.AppHost.Services;

    public static MainWindow MainWindow { get; set; } = null!;

    public static DispatcherQueue DispatcherQueue { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        // Increase thread pool size for bad async code
        // TODO: Remove this when refactoring is completed
        ThreadPool.SetMinThreads(20, 20);
        ThreadPool.SetMaxThreads(20, 20);

        // Global exception handler
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            ProcessException(e.Exception);
        };

        DispatcherQueue = DispatcherQueue.GetForCurrentThread();
        App.GetService<AppearanceService>().ApplyDisplayTheme();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        string[] cmdargs = Environment.GetCommandLineArgs();

        // TODO: Move to UI services
        App.GetService<LaunchSessions>(); // Init global launch sessions collection
        App.GetService<MessengerService>().SubscribeEvents();

        if (cmdargs.Length > 1 && cmdargs[1].Equals("/quick-launch"))
        {
            App.GetService<JumpListService>().LaunchFromJumpList(cmdargs[2]);
            return;
        }

        try { App.GetService<IActivationService>().ActivateWindow("MainWindow"); }
        catch (Exception e) { ProcessException(e); }
    }

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
        if (App.MainWindow is not null)
        {
            App.DispatcherQueue?.TryEnqueue(async () =>
            {
                try
                {
                    var dialog = new ExceptionDialog(errorMessage) { XamlRoot = MainWindow.Content.XamlRoot };
                    await dialog.ShowAsync();
                }
                catch
                {
                    var window = new Window() { Title = "Fluent Launcher" };
                    window.Content = new ExceptionPage(errorMessage);
                    window.Activate();
                }
            });
        }
        else
        {
            var window = new Window() { Title = "Fluent Launcher" };
            window.Content = new ExceptionPage(errorMessage);
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

    public static T GetService<T>() where T : notnull // TODO: Rename to GetRequiredService
        => Services.GetRequiredService<T>();
}

using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Utils;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    public static IServiceProvider Services => Program.AppHost.Services;

    public static MainWindow MainWindow { get; set; } = null!;

    public static DispatcherQueue DispatcherQueue { get; private set; } = null!;

    public static Windows.ApplicationModel.PackageVersion Version => Windows.ApplicationModel.Package.Current.Id.Version;

    public App()
    {
        InitializeComponent();
        ConfigureApplication();
    }

    void ConfigureApplication()
    {
        //Fix https://github.com/MCLF-CN/docs/issues/2 
        HttpUtils.HttpClient.DefaultRequestHeaders.UserAgent.Clear();
        HttpUtils.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Natsurainko.FluentLauncher", Version.GetVersionString()));

        DispatcherQueue = DispatcherQueue.GetForCurrentThread();

        App.GetService<MessengerService>().SubscribeEvents();
        App.GetService<AppearanceService>().RegisterApp(this);
        App.GetService<QuickLaunchService>().CleanRemovedJumpListItem();

        // Global exception handler
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            ProcessException(e.Exception);
        };
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // 确保单例应用程序启动
        var mainInstance = AppInstance.FindOrRegisterForKey("Main");
        mainInstance.Activated += (object? sender, AppActivationArguments e) =>
        {
            DispatcherQueue.TryEnqueue(() => MainWindow?.Activate());

            if (e.Data is Windows.ApplicationModel.Activation.LaunchActivatedEventArgs redirectedArgs)
                App.GetService<QuickLaunchService>().LaunchFromActivatedEventArgs(redirectedArgs.Arguments.Split(' '));
        };

        if (!mainInstance.IsCurrent)
        {
            //Redirect the activation (and args) to the "main" instance, and exit.
            var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            await mainInstance.RedirectActivationToAsync(activatedEventArgs);
            Process.GetCurrentProcess().Kill();
            return;
        }

        try
        {
            IWindowService mainWindowService = App.GetService<IActivationService>().ActivateWindow("MainWindow");
        }
        catch (Exception e)
        {
            ProcessException(e);
        }
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
                    await new ExceptionDialog(errorMessage).ShowAsync();
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

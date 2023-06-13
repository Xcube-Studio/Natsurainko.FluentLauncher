using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.CrossProcess;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Storage;
using Natsurainko.FluentLauncher.Views.Common;
using Microsoft.Extensions.DependencyInjection;
using Natsurainko.FluentLauncher.Services;
using AppSettingsManagement;
using AppSettingsManagement.Windows;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Views;
using System.Text.Json;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;

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
    public static IServiceProvider Services { get; } = ConfigureServices();
    public static T GetService<T>() => Services.GetService<T>();
    public static MainWindow MainWindow { get; private set; }

    //public static Configuration Configuration { get; private set; } = Configuration.Load();

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

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        App.GetService<MessengerService>().SubscribeEvents();
        try
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
        catch (Exception e)
        {
            ProcessException(e);
        }
    }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<OfficialNewsService>();
        services.AddSingleton<CurseForgeModService>();
        services.AddSingleton<AccountService>();
        services.AddSingleton<LocalStorageService>();
        services.AddSingleton<MessengerService>();

        // Settings service
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsStorage, WinRTSettingsStorage>();

        //ViewModels
        services.AddSingleton<ViewModels.Activities.NewsViewModel>();
        services.AddSingleton<ViewModels.Downloads.CurseForgeViewModel>();

        services.AddTransient<ViewModels.Settings.AppearanceViewModel>();
        services.AddTransient<ViewModels.Settings.DownloadViewModel>();
        services.AddTransient<ViewModels.Settings.AccountViewModel>();
        services.AddTransient<ViewModels.Settings.LaunchViewModel>();

        services.AddTransient<ViewModels.OOBE.LanguageViewModel>();
        services.AddTransient<ViewModels.OOBE.BasicViewModel>();
        services.AddTransient<ViewModels.OOBE.AccountViewModel>();
        services.AddTransient<ViewModels.OOBE.GetStartedViewModel>();

        services.AddTransient<ViewModels.Cores.CoresViewModel>();
        services.AddTransient<ViewModels.Home.HomeViewModel>();

        return services.BuildServiceProvider();
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
            App.MainWindow.DispatcherQueue?.TryEnqueue(async () =>
            {
                var dialog = new ExceptionDialog(errorMessage) { XamlRoot = MainWindow.Content.XamlRoot };
                await dialog.ShowAsync();
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
}

using AppSettingsManagement;
using AppSettingsManagement.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.SystemServices;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Pages;
using Natsurainko.FluentLauncher.Services.UI.Windows;
using Natsurainko.FluentLauncher.ViewModels;
using Natsurainko.FluentLauncher.ViewModels.Activities;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using System;
using System.Text;
using System.Threading;

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    public static IServiceProvider Services { get; } = ConfigureServices();
    public static T GetService<T>() => Services.GetService<T>();
    public static MainWindow MainWindow { get; set; }
    public static DispatcherQueue DispatcherQueue { get; private set; }

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

    private static IPageProvider BuildPageProvider(IServiceProvider sp) => WinUIPageProvider.GetBuilder(sp)
        // OOBE
        .WithPage<Views.OOBE.OOBENavigationPage, ViewModels.OOBE.OOBEViewModel>("OOBENavigationPage")
        .WithPage<Views.OOBE.AccountPage>("OOBEAccountPage")
        .WithPage<Views.OOBE.MinecraftFolderPage>("OOBEMinecraftFolderPage")
        .WithPage<Views.OOBE.JavaPage>("OOBEJavaPage")
        .WithPage<Views.OOBE.GetStartedPage>("OOBEGetStartedPage")
        .WithPage<Views.OOBE.LanguagePage>("OOBELanguagePage")

        // Main
        .WithPage<ShellPage, ShellViewModel>("ShellPage")

        // Home page
        .WithPage<Views.Home.HomePage, ViewModels.Home.HomeViewModel>("HomePage")
        .WithPage<Views.Home.NewHomePage, ViewModels.Home.HomeViewModel>("NewHomePage")

        // Cores page
        .WithPage<Views.Cores.CoresPage, ViewModels.Cores.CoresViewModel>("CoresPage")
        .WithPage<Views.Cores.ManageNavigationPage, ViewModels.Cores.ManageNavigationViewModel>("CoresManageNavigationPage")
        .WithPage<Views.Cores.Manage.CoreSettingsPage, ViewModels.Cores.Manage.CoreSettingsViewModel>("CoreSettingsPage")
        .WithPage<Views.Cores.Manage.CoreModsPage, ViewModels.Cores.Manage.CoreModsViewModel>("CoreModsPage")
        .WithPage<Views.Cores.Manage.CoreStatisticPage, ViewModels.Cores.Manage.CoreStatisticViewModel>("CoreStatisticPage")

        // Activities page
        .WithPage<Views.Activities.ActivitiesNavigationPage, ViewModels.Activities.ActivitiesNavigationViewModel>("ActivitiesNavigationPage")
        .WithPage<Views.Activities.LaunchPage, ViewModels.Activities.LaunchViewModel>("LaunchTasksPage")
        .WithPage<Views.Activities.DownloadPage, ViewModels.Activities.DownloadViewModel>("DownloadTasksPage")
        .WithPage<Views.Activities.NewsPage, ViewModels.Activities.NewsViewModel>("NewsPage")

        // Resources download page
        .WithPage<Views.Downloads.DownloadsPage, ViewModels.Downloads.DownloadsViewModel>("ResourcesDownloadPage")
        .WithPage<Views.Downloads.ResourcesSearchPage, ViewModels.Downloads.ResourcesSearchViewModel>("ResourcesSearchPage")
        .WithPage<Views.Downloads.CoreInstallWizardPage, ViewModels.Downloads.CoreInstallWizardViewModel>("CoreInstallWizardPage")
        .WithPage<Views.Downloads.ResourceItemPage, ViewModels.Downloads.ResourceItemViewModel>("ResourceItemPage") // Configures VM for itself

        // Settings
        .WithPage<Views.Settings.NavigationPage, ViewModels.Settings.SettingsNavigationViewModel>("SettingsNavigationPage")
        .WithPage<Views.Settings.LaunchPage, ViewModels.Settings.LaunchViewModel>("LaunchSettingsPage")
        .WithPage<Views.Settings.AccountPage, ViewModels.Settings.AccountViewModel>("AccountSettingsPage")
        .WithPage<Views.Settings.DownloadPage, ViewModels.Settings.DownloadViewModel>("DownloadSettingsPage")
        .WithPage<Views.Settings.AppearancePage, ViewModels.Settings.AppearanceViewModel>("AppearanceSettingsPage")
        .WithPage<Views.Settings.AboutPage, ViewModels.Settings.AboutViewModel>("AboutPage")

        .Build();

    private static IActivationService BuildActivationService() => WinUIActivationService.GetBuilder(Services)
        .WithSingleInstanceWindow<MainWindow>("MainWindow")
        // .WithMultiInstanceWindow<LogWindow>("LogWindow")
        .Build();

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // UI services
        services.AddSingleton<IPageProvider>(sp => BuildPageProvider(sp));
        services.AddSingleton<IActivationService>(_ => BuildActivationService());
        services.AddScoped<INavigationService, NavigationService>(); // A scope is created for each window or page that supports navigation.

        // Settings service
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsStorage, WinRTSettingsStorage>();

        // FluentCore Services
        services.AddSingleton<GameService>();
        services.AddSingleton<LaunchService>();
        services.AddSingleton<AccountService>();
        services.AddSingleton<DownloadService>();

        // Services
        services.AddSingleton<LocalStorageService>();
        services.AddSingleton<MessengerService>();
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<AppearanceService>();
        services.AddSingleton<SkinCacheService>();
        services.AddSingleton<InterfaceCacheService>();
        services.AddSingleton<JumpListService>();

        // Windows
        services.AddScoped<MainWindow>();

        // ViewModels
        services.AddTransient<ViewModels.OOBE.OOBEViewModel>();

        services.AddSingleton<ViewModels.Activities.LaunchSessions>();
        services.AddTransient<ViewModels.Activities.ActivitiesNavigationViewModel>();
        services.AddTransient<ViewModels.Activities.NewsViewModel>();
        services.AddTransient<ViewModels.Activities.LaunchViewModel>();
        services.AddTransient<ViewModels.Activities.DownloadViewModel>();

        services.AddTransient<ViewModels.Common.SwitchAccountDialogViewModel>();

        services.AddTransient<ViewModels.Settings.SettingsNavigationViewModel>();
        services.AddTransient<ViewModels.Settings.AppearanceViewModel>();
        services.AddTransient<ViewModels.Settings.DownloadViewModel>();
        services.AddTransient<ViewModels.Settings.AccountViewModel>();
        services.AddTransient<ViewModels.Settings.LaunchViewModel>();
        services.AddTransient<ViewModels.Settings.AboutViewModel>();

        services.AddTransient<ViewModels.Cores.CoresViewModel>();
        services.AddTransient<ViewModels.Cores.ManageNavigationViewModel>();
        services.AddTransient<ViewModels.Cores.Manage.CoreModsViewModel>();
        services.AddTransient<ViewModels.Cores.Manage.CoreSettingsViewModel>();
        services.AddTransient<ViewModels.Cores.Manage.CoreStatisticViewModel>();

        services.AddTransient<ViewModels.Home.HomeViewModel>();

        services.AddTransient<ViewModels.Downloads.DownloadsViewModel>();
        services.AddTransient<ViewModels.Downloads.ResourcesSearchViewModel>();
        services.AddTransient<ViewModels.Downloads.ResourceItemViewModel>();
        services.AddTransient<ViewModels.Downloads.CoreInstallWizardViewModel>();

        services.AddTransient<ViewModels.ShellViewModel>();

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
}

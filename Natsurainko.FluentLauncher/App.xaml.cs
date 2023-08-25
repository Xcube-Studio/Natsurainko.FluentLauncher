﻿using AppSettingsManagement;
using AppSettingsManagement.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using System;
using System.Text;
using Natsurainko.FluentLauncher.Services.UI.Windows;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Pages;
using Natsurainko.FluentLauncher.Views.OOBE;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using Natsurainko.FluentLauncher.Views.Home;
using Natsurainko.FluentLauncher.ViewModels.Home;
using Natsurainko.FluentLauncher.Views.Cores;
using Natsurainko.FluentLauncher.ViewModels.Cores;
using Natsurainko.FluentLauncher.Views.Activities;
using Natsurainko.FluentLauncher.ViewModels.Activities;
using Windows.UI.ApplicationSettings;
using Natsurainko.FluentLauncher.Views.Settings;

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    public static IServiceProvider Services { get; } = ConfigureServices();
    public static T GetService<T>() => Services.GetService<T>();
    public static MainWindow MainWindow { get; set; }
    public static DispatcherQueue DispatcherQueue { get; private set; }

    private static IPageProvider PageProvider;

    public App()
    {
        InitializeComponent();

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

        if (cmdargs.Length > 1 && cmdargs[1].Equals("/quick-launch"))
        {
            App.GetService<LaunchService>().LaunchFromJumpList(cmdargs[2]);
            return;
        }

        // Demo
        var activationService = WinUIActivationService.GetBuilder(Services)
            .WithSingleInstanceWindow<MainWindow>("MainWindow")
            //.WithSingleInstanceWindow<OOBEWindow>("OOBEWindow")
            //.WithMultiInstanceWindow<LogWindow>("LogWindow")
            .Build();

        PageProvider = WinUIPageProvider.GetBuilder(Services)
            .WithPage<OOBENavigationPage, OOBENavigationViewModel>("OOBENavigationPage")
            .WithPage<ShellPage>("ShellPage")
            .WithPage<HomePage, HomeViewModel>("HomePage")
            .WithPage<NewHomePage, HomeViewModel>("NewHomePage")
            .WithPage<CoresPage, CoresViewModel>("CoresPage")
            .WithPage<ActivitiesNavigationPage>("ActivitiesNavigationPage")
            .WithPage<Views.Activities.DownloadPage, DownloadViewModel>("DownloadsPage")
            .WithPage<NavigationPage>("SettingsNavigationPage")
            .Build();

        App.GetService<MessengerService>().SubscribeEvents();

        try
        {
            activationService.ActivateWindow("MainWindow");
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

        // UI services
        services.AddScoped<INavigationService, NavigationService>();
        services.AddSingleton<IPageProvider>(_ => PageProvider);
        services.AddTransient<ShellPage>();
        services.AddTransient<HomePage>();

        // Settings service
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsStorage, WinRTSettingsStorage>();

        // FluentCore Services
        services.AddSingleton<GameService>();
        services.AddSingleton<LaunchService>();
        services.AddSingleton<AccountService>();
        services.AddSingleton<DownloadService>();

        // Services
        //services.AddSingleton<CurseForgeModService>();
        services.AddSingleton<LocalStorageService>();
        services.AddSingleton<MessengerService>();
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<AppearanceService>();
        services.AddSingleton<SkinCacheService>();
        services.AddSingleton<InterfaceCacheService>();

        //ViewModels

        /// Activities
        services.AddSingleton<ViewModels.Activities.NewsViewModel>();
        services.AddTransient<ViewModels.Activities.LaunchViewModel>();
        //services.AddTransient<ViewModels.Activities.DownloadViewModel>();
        //services.AddSingleton<ViewModels.Downloads.CurseForgeViewModel>();

        services.AddTransient<ViewModels.Common.SwitchAccountDialogViewModel>();

        services.AddTransient<ViewModels.Settings.AppearanceViewModel>();
        services.AddTransient<ViewModels.Settings.DownloadViewModel>();
        services.AddTransient<ViewModels.Settings.AccountViewModel>();
        services.AddTransient<ViewModels.Settings.LaunchViewModel>();

        services.AddTransient<ViewModels.OOBE.LanguageViewModel>();
        services.AddTransient<ViewModels.OOBE.BasicViewModel>();
        services.AddTransient<ViewModels.OOBE.AccountViewModel>();
        services.AddTransient<ViewModels.OOBE.GetStartedViewModel>();

        services.AddScoped<ViewModels.Cores.CoresViewModel>();
        services.AddScoped<ViewModels.Home.HomeViewModel>();

        services.AddScoped<MainWindow>();

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

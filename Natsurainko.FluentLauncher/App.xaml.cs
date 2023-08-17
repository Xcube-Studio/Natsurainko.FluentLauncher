﻿using AppSettingsManagement;
using AppSettingsManagement.Windows;
using Microsoft.Extensions.DependencyInjection;
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

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    public static IServiceProvider Services { get; } = ConfigureServices();
    public static T GetService<T>() => Services.GetService<T>();
    public static MainWindow MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        // Global exception handler
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            ProcessException(e.Exception);
        };

        App.GetService<AppearanceService>().ApplyDisplayTheme();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Demo
        var activationService = WinUIActivationService.GetBuilder(Services)
            .WithSingleInstanceWindow<MainWindow>("MainWindow")
            //.WithSingleInstanceWindow<OOBEWindow>("OOBEWindow")
            //.WithMultiInstanceWindow<LogWindow>("LogWindow")
            .Build();

        App.GetService<MessengerService>().SubscribeEvents();

        try
        {
            activationService.ActivateWindow("MainWindow");
            MainWindow = App.GetService<MainWindow>();
            App.GetService<AppearanceService>().ApplyBackgroundAtWindowCreated(MainWindow);
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

        services.AddTransient<ViewModels.Cores.CoresViewModel>();
        services.AddTransient<ViewModels.Home.HomeViewModel>();

        services.AddSingleton<MainWindow>();

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

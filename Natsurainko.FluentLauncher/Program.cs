using Natsurainko.FluentLauncher;
using System;
using Microsoft.Extensions.Hosting;
using FluentLauncher.Infra.WinUI.AppHost;
using Microsoft.Extensions.DependencyInjection;
using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Navigation;
using FluentLauncher.Infra.WinUI.Pages;
using FluentLauncher.Infra.WinUI.Settings;
using FluentLauncher.Infra.WinUI.Windows;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.SystemServices;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI;

using Views = Natsurainko.FluentLauncher.Views;
using ViewModels = Natsurainko.FluentLauncher.ViewModels;

var builder = WinUIApplication.CreateBuilder(() => new App());

//builder.Configuration.AddJsonFile("appsettings.json", optional: true);
//builder.Configuration.AddCommandLine(args);

//builder.Services.Add(...);

//builder.Logging...

builder.UseExtendedWinUIServices();

// Configure WinUI windows
builder.Windows.WithSingleInstanceWindow<Views.MainWindow>("MainWindow");

#region Services

var services = builder.Services;
// UI services
services.AddSingleton<IPageProvider>(sp => BuildPageProvider(sp));
services.AddScoped<INavigationService, WinUINavigationService>(); // A scope is created for each window or page that supports navigation.

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

#endregion

// Build and run the app
var app = builder.Build();
AppHost = app.Host;

await app.RunAsync();


static IPageProvider BuildPageProvider(IServiceProvider sp) => WinUIPageProvider.GetBuilder(sp)
    // OOBE
    .WithPage<Views.OOBE.OOBENavigationPage, ViewModels.OOBE.OOBEViewModel>("OOBENavigationPage")
    .WithPage<Views.OOBE.AccountPage>("OOBEAccountPage")
    .WithPage<Views.OOBE.MinecraftFolderPage>("OOBEMinecraftFolderPage")
    .WithPage<Views.OOBE.JavaPage>("OOBEJavaPage")
    .WithPage<Views.OOBE.GetStartedPage>("OOBEGetStartedPage")
    .WithPage<Views.OOBE.LanguagePage>("OOBELanguagePage")

    // Main
    .WithPage<Views.ShellPage, ViewModels.ShellViewModel>("ShellPage")

    // Home page
    .WithPage<Views.Home.HomePage, ViewModels.Home.HomeViewModel>("HomePage")

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


public partial class Program
{
    public static IHost AppHost { get; private set; } = null!;
}

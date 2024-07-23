using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.WinUI.AppHost;
using FluentLauncher.Infra.WinUI.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsurainko.FluentLauncher;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.SystemServices;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using System;
using ViewModels = Natsurainko.FluentLauncher.ViewModels;
using Views = Natsurainko.FluentLauncher.Views;

var builder = WinUIApplication.CreateBuilder(() => new App());

//builder.Configuration.AddJsonFile("appsettings.json", optional: true);
//builder.Configuration.AddCommandLine(args);

//builder.Logging...

builder.UseExtendedWinUIServices();

// Configure WinUI windows
builder.Windows.AddSingleInstanceWindow<Views.MainWindow>("MainWindow");

#region Configure WinUI pages

var pages = builder.Pages;

// OOBE
pages.WithPage<Views.OOBE.OOBENavigationPage, ViewModels.OOBE.OOBEViewModel>("OOBENavigationPage");
pages.WithPage<Views.OOBE.AccountPage>("OOBEAccountPage");
pages.WithPage<Views.OOBE.MinecraftFolderPage>("OOBEMinecraftFolderPage");
pages.WithPage<Views.OOBE.JavaPage>("OOBEJavaPage");
pages.WithPage<Views.OOBE.GetStartedPage>("OOBEGetStartedPage");
pages.WithPage<Views.OOBE.LanguagePage>("OOBELanguagePage");

// Main
pages.WithPage<Views.ShellPage, ViewModels.ShellViewModel>("ShellPage");

// Home page
pages.WithPage<Views.Home.HomePage, ViewModels.Home.HomeViewModel>("HomePage");

// Cores page
pages.WithPage<Views.Cores.CoresPage, ViewModels.Cores.CoresViewModel>("CoresPage");

pages.WithPage<Views.Cores.Manage.NavigationPage, ViewModels.Cores.Manage.NavigationViewModel>("CoreManage/Navigation");
pages.WithPage<Views.Cores.Manage.DefaultPage, ViewModels.Cores.Manage.DefaultViewModel>("CoreManage/Default");
pages.WithPage<Views.Cores.Manage.ConfigPage>("CoreManage/Config");
pages.WithPage<Views.Cores.Manage.ModPage>("CoreManage/Mod");
pages.WithPage<Views.Cores.Manage.MapPage>("CoreManage/Map");
pages.WithPage<Views.Cores.Manage.StatisticPage>("CoreManage/Statistic");

/*
pages.WithPage<Views.Cores.ManageNavigationPage, ViewModels.Cores.ManageNavigationViewModel>("CoresManageNavigationPage");
pages.WithPage<Views.Cores.Manage.CoreSettingsPage, ViewModels.Cores.Manage.CoreSettingsViewModel>("CoreSettingsPage");
pages.WithPage<Views.Cores.Manage.CoreModsPage, ViewModels.Cores.Manage.CoreModsViewModel>("CoreModsPage");
pages.WithPage<Views.Cores.Manage.CoreStatisticPage, ViewModels.Cores.Manage.CoreStatisticViewModel>("CoreStatisticPage");*/

// Activities page
pages.WithPage<Views.Activities.ActivitiesNavigationPage, ViewModels.Activities.ActivitiesNavigationViewModel>("ActivitiesNavigationPage");
pages.WithPage<Views.Activities.LaunchPage, ViewModels.Activities.LaunchViewModel>("LaunchTasksPage");
pages.WithPage<Views.Activities.DownloadPage, ViewModels.Activities.DownloadViewModel>("DownloadTasksPage");
pages.WithPage<Views.Activities.NewsPage, ViewModels.Activities.NewsViewModel>("NewsPage");

// Resources download page
pages.WithPage<Views.Downloads.DownloadsPage, ViewModels.Downloads.DownloadsViewModel>("ResourcesDownloadPage");
pages.WithPage<Views.Downloads.ResourcesSearchPage, ViewModels.Downloads.ResourcesSearchViewModel>("ResourcesSearchPage");
pages.WithPage<Views.Downloads.CoreInstallWizardPage, ViewModels.Downloads.CoreInstallWizardViewModel>("CoreInstallWizardPage");
pages.WithPage<Views.Downloads.ResourceItemPage, ViewModels.Downloads.ResourceItemViewModel>("ResourceItemPage");

// Settings
pages.WithPage<Views.Settings.NavigationPage, ViewModels.Settings.NavigationViewModel>("Settings/Navigation");
pages.WithPage<Views.Settings.DefaultPage, ViewModels.Settings.DefaultViewModel>("Settings/Default");
pages.WithPage<Views.Settings.LaunchPage, ViewModels.Settings.LaunchViewModel>("Settings/Launch");
pages.WithPage<Views.Settings.AccountPage, ViewModels.Settings.AccountViewModel>("Settings/Account");
pages.WithPage<Views.Settings.DownloadPage, ViewModels.Settings.DownloadViewModel>("Settings/Download");
pages.WithPage<Views.Settings.AppearancePage, ViewModels.Settings.AppearanceViewModel>("Settings/Appearance");
pages.WithPage<Views.Settings.AboutPage, ViewModels.Settings.AboutViewModel>("Settings/About");
pages.WithPage<Views.Settings.SkinPage, ViewModels.Settings.SkinViewModel>("Settings/Account/Skin");

#endregion

#region Services

var services = builder.Services;

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
services.AddSingleton<CacheSkinService>();
services.AddSingleton<InterfaceCacheService>();
services.AddSingleton<JumpListService>();

// ViewModels
services.AddSingleton<ViewModels.Activities.LaunchSessions>();
services.AddTransient<ViewModels.Common.SwitchAccountDialogViewModel>();

#endregion

// Build and run the app
var app = builder.Build();
AppHost = app.Host;

await app.RunAsync();

public partial class Program
{
    public static IHost AppHost { get; private set; } = null!;
}

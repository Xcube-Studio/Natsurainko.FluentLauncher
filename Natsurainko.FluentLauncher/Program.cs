using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.UI.Notification;
using FluentLauncher.Infra.WinUI.AppHost;
using FluentLauncher.Infra.WinUI.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsurainko.FluentLauncher;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.CommandLine;
using ViewModels = Natsurainko.FluentLauncher.ViewModels;
using Views = Natsurainko.FluentLauncher.Views;

var builder = WinUIApplication.CreateBuilder(() => new App());

//builder.Configuration.AddJsonFile("appsettings.json", optional: true);
//builder.Configuration.AddCommandLine(args);

//builder.Logging...

builder.UseExtendedWinUIServices();

#if ENABLE_LOAD_EXTENSIONS
builder.UseApplicationExtensionHost();
#endif

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

// Instances page

pages.WithPage<Views.Instances.NavigationPage, ViewModels.Instances.NavigationViewModel>("Instances/Navigation");
pages.WithPage<Views.Instances.DefaultPage, ViewModels.Instances.DefaultViewModel>("Instances/Default");
pages.WithPage<Views.Instances.InstancePage, ViewModels.Instances.InstanceViewModel>("Instances/Instance");
pages.WithPage<Views.Instances.ConfigPage, ViewModels.Instances.ConfigViewModel>("Instances/Config");
pages.WithPage<Views.Instances.ModPage, ViewModels.Instances.ModViewModel>("Instances/Mod");
pages.WithPage<Views.Instances.SavePage, ViewModels.Instances.SaveViewModel>("Instances/Save");

// News page
pages.WithPage<Views.News.NavigationPage, ViewModels.News.NavigationViewModel>("News/Navigation");
pages.WithPage<Views.News.DefaultPage, ViewModels.News.DefaultViewModel>("News/Default");
pages.WithPage<Views.News.NotePage, ViewModels.News.NoteViewModel>("News/Note");

// Resources download page

pages.WithPage<Views.Downloads.Instances.NavigationPage, ViewModels.Downloads.Instances.NavigationViewModel>("InstancesDownload/Navigation");
pages.WithPage<Views.Downloads.Instances.DefaultPage, ViewModels.Downloads.Instances.DefaultViewModel>("InstancesDownload/Default");
pages.WithPage<Views.Downloads.Instances.InstallPage, ViewModels.Downloads.Instances.InstallViewModel>("InstancesDownload/Install");

pages.WithPage<Views.Downloads.Mods.NavigationPage, ViewModels.Downloads.Mods.NavigationViewModel>("ModsDownload/Navigation");
pages.WithPage<Views.Downloads.Mods.DefaultPage, ViewModels.Downloads.Mods.DefaultViewModel>("ModsDownload/Default");
pages.WithPage<Views.Downloads.Mods.ModPage, ViewModels.Downloads.Mods.ModViewModel>("ModsDownload/Mod");

// Tasks page
pages.WithPage<Views.Tasks.LaunchPage, ViewModels.Tasks.LaunchViewModel>("Tasks/Launch");
pages.WithPage<Views.Tasks.DownloadPage, ViewModels.Tasks.DownloadViewModel>("Tasks/Download");

// Settings page
pages.WithPage<Views.Settings.NavigationPage, ViewModels.Settings.NavigationViewModel>("Settings/Navigation");
pages.WithPage<Views.Settings.DefaultPage, ViewModels.Settings.DefaultViewModel>("Settings/Default");
pages.WithPage<Views.Settings.LaunchPage, ViewModels.Settings.LaunchViewModel>("Settings/Launch");
pages.WithPage<Views.Settings.AccountPage, ViewModels.Settings.AccountViewModel>("Settings/Account");
pages.WithPage<Views.Settings.DownloadPage, ViewModels.Settings.DownloadViewModel>("Settings/Download");
pages.WithPage<Views.Settings.AppearancePage, ViewModels.Settings.AppearanceViewModel>("Settings/Appearance");
pages.WithPage<Views.Settings.AboutPage, ViewModels.Settings.AboutViewModel>("Settings/About");
pages.WithPage<Views.Settings.SkinPage, ViewModels.Settings.SkinViewModel>("Settings/Account/Skin");

#endregion

#region Configure WinUI dialogs

var dialogs = builder.Dialogs;

dialogs.WithDialog<Views.Dialogs.AddArgumentDialog, ViewModels.Dialogs.AddArgumentDialogViewModel>("AddVmArgumentDialog");
dialogs.WithDialog<Views.Dialogs.AuthenticateDialog, ViewModels.Dialogs.AuthenticateDialogViewModel>("AuthenticationWizardDialog");
dialogs.WithDialog<Views.Dialogs.DeleteInstanceDialog, ViewModels.Dialogs.DeleteInstanceDialogViewModel>("DeleteInstanceDialog");
dialogs.WithDialog<Views.Dialogs.SwitchAccountDialog, ViewModels.Dialogs.SwitchAccountDialogViewModel>("SwitchAccountDialog");
dialogs.WithDialog<Views.Dialogs.UploadSkinDialog, ViewModels.Dialogs.UploadSkinDialogViewModel>("UploadSkinDialog");

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
dialogs.WithDialog<Views.Dialogs.UpdateDialog, ViewModels.Dialogs.UpdateDialogViewModel>("UpdateDialog");
#endif

#endregion

#region Services

var services = builder.Services;

services.UseHttpClient();
services.UseResourceClients();

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
services.AddSingleton<Natsurainko.FluentLauncher.Services.UI.NotificationService>();
services.AddSingleton<AppearanceService>();
services.AddSingleton<CacheSkinService>();
services.AddSingleton<CacheInterfaceService>();
services.AddSingleton<QuickLaunchService>();
services.AddSingleton<SearchProviderService>();
services.AddSingleton<InstanceConfigService>();

// UI Services
services.AddSingleton<InfoBarPresenter>();
services.AddSingleton<SystemNotificationPresenter>();
services.AddSingleton<TeachingTipPresenter>();

services.AddSingleton<INotificationService, Natsurainko.FluentLauncher.Services.UI.Notification.NotificationService>();


#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
services.AddSingleton<UpdateService>();
#endif

#endregion

// Build and run the app
var app = builder.Build();
AppHost = app.Host;

await BuildRootCommand(app).InvokeAsync(args);

public partial class Program
{
    public static IHost AppHost { get; private set; } = null!;

    public static Option<string> MinecraftFolderOption { get; } = new (name: "--minecraftFolder") { IsRequired = true };

    public static Option<string> InstanceIdOption { get; } = new(name: "--instanceId") { IsRequired = true };

    public static RootCommand BuildRootCommand(WinUIApplication application)
    {
        var rootCommand = new RootCommand();
        rootCommand.SetHandler(async () => await application.RunAsync());
        rootCommand.Add(BuildSubCommand());

        return rootCommand;
    }

    public static Command BuildSubCommand()
    {
        var quickLaunchCommand = new Command("quickLaunch");
        quickLaunchCommand.AddOption(MinecraftFolderOption);
        quickLaunchCommand.AddOption(InstanceIdOption);

        quickLaunchCommand.SetHandler(async (folder, instanceId) => 
            await AppHost.Services.GetService<QuickLaunchService>()!.LaunchFromArguments(folder, instanceId), 
            MinecraftFolderOption, InstanceIdOption);

        return quickLaunchCommand;
    }
}

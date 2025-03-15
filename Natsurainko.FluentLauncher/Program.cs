using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.WinUI.AppHost;
using FluentLauncher.Infra.WinUI.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Natsurainko.FluentLauncher;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Nrk.FluentCore.Resources;
using System;
using System.CommandLine;
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

pages.WithPage<Views.Cores.NavigationPage, ViewModels.Cores.NavigationViewModel>("Cores/Navigation");
pages.WithPage<Views.Cores.DefaultPage, ViewModels.Cores.DefaultViewModel>("Cores/Default");
pages.WithPage<Views.Cores.InstancePage, ViewModels.Cores.InstanceViewModel>("Cores/Instance");
pages.WithPage<Views.Cores.ConfigPage, ViewModels.Cores.ConfigViewModel>("Cores/Config");
pages.WithPage<Views.Cores.ModPage, ViewModels.Cores.ModViewModel>("Cores/Mod");
pages.WithPage<Views.Cores.SavePage, ViewModels.Cores.SaveViewModel>("Cores/Save");

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

dialogs.WithDialog<Views.Common.AddVmArgumentDialog, ViewModels.Common.AddVmArgumentDialogViewModel>("AddVmArgumentDialog");
dialogs.WithDialog<Views.Common.AuthenticationWizardDialog, ViewModels.Common.AuthenticationWizardDialogViewModel>("AuthenticationWizardDialog");
dialogs.WithDialog<Views.Common.DeleteInstanceDialog, ViewModels.Common.DeleteInstanceDialogViewModel>("DeleteInstanceDialog");
dialogs.WithDialog<Views.Common.SwitchAccountDialog, ViewModels.Common.SwitchAccountDialogViewModel>("SwitchAccountDialog");
dialogs.WithDialog<Views.Common.UploadSkinDialog, ViewModels.Common.UploadSkinDialogViewModel>("UploadSkinDialog");

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
dialogs.WithDialog<Views.Common.UpdateDialog, ViewModels.Common.UpdateDialogViewModel>("UpdateDialog");
#endif

#endregion

#region Services

var services = builder.Services;

var dispatcherQueue = DispatcherQueue.GetForCurrentThread();


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
services.AddSingleton<CacheInterfaceService>();
services.AddSingleton<QuickLaunchService>();
services.AddSingleton<SearchProviderService>();
services.AddSingleton<InstanceConfigService>();

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
services.AddSingleton<UpdateService>();
#endif

// ModClient
services.AddSingleton<ModrinthClient>();
services.AddSingleton<CurseForgeClient>(_ => new CurseForgeClient("$2a$10$lf9.hHl3PMJ4d3BisICcAOX91uT/mM9/VPDfzpg7r3C/Y8cXIRTNm"));

// ViewModels
services.AddTransient<ViewModels.Common.SwitchAccountDialogViewModel>();

#endregion

// Build and run the app
var app = builder.Build();
AppHost = app.Host;

await BuildRootCommand(app).InvokeAsync(args);
//await app.RunAsync();

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

using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.UI.Notification;
using FluentLauncher.Infra.WinUI.AppHost;
using FluentLauncher.Infra.WinUI.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Web;
using ViewModels = Natsurainko.FluentLauncher.ViewModels;
using Views = Natsurainko.FluentLauncher.Views;

var builder = WinUIApplication<App>.CreateBuilder();

builder.UseSerilog();
builder.UseExtendedWinUIServices();

#if ENABLE_LOAD_EXTENSIONS
builder.UseApplicationExtensionHost();
#endif

// Configure WinUI windows
builder.Windows.AddSingleInstanceWindow<Views.MainWindow>("MainWindow");
builder.Windows.AddMultiInstanceWindow<Views.LoggerWindow>("LoggerWindow");

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
pages.WithPage<Views.Downloads.ResourceDefaultPage, ViewModels.Downloads.Mods.DefaultViewModel>("ModsDownload/Default");
pages.WithPage<Views.Downloads.ResourcePage, ViewModels.Downloads.ResourceViewModel>("ModsDownload/Resource");

pages.WithPage<Views.Downloads.Modpacks.NavigationPage, ViewModels.Downloads.Modpacks.NavigationViewModel>("ModpacksDownload/Navigation");
pages.WithPage<Views.Downloads.ResourceDefaultPage, ViewModels.Downloads.Modpacks.DefaultViewModel>("ModpacksDownload/Default");
pages.WithPage<Views.Downloads.ResourcePage, ViewModels.Downloads.ResourceViewModel>("ModpacksDownload/Resource");

pages.WithPage<Views.Downloads.TexturePacks.NavigationPage, ViewModels.Downloads.TexturePacks.NavigationViewModel>("TexturePacksDownload/Navigation");
pages.WithPage<Views.Downloads.ResourceDefaultPage, ViewModels.Downloads.TexturePacks.DefaultViewModel>("TexturePacksDownload/Default");
pages.WithPage<Views.Downloads.ResourcePage, ViewModels.Downloads.ResourceViewModel>("TexturePacksDownload/Resource");

pages.WithPage<Views.Downloads.Shaders.NavigationPage, ViewModels.Downloads.Shaders.NavigationViewModel>("ShadersDownload/Navigation");
pages.WithPage<Views.Downloads.ResourceDefaultPage, ViewModels.Downloads.Shaders.DefaultViewModel>("ShadersDownload/Default");
pages.WithPage<Views.Downloads.ResourcePage, ViewModels.Downloads.ResourceViewModel>("ShadersDownload/Resource");

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
pages.WithPage<Views.Settings.LauncherPage, ViewModels.Settings.LauncherViewModel>("Settings/Launcher");
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
dialogs.WithDialog<Views.Dialogs.SelectColorDialog, ViewModels.Dialogs.SelectColorDialogViewModel>("SelectColorDialog");
dialogs.WithDialog<Views.Dialogs.SelectImageThemeColorDialog, ViewModels.Dialogs.SelectImageThemeColorDialogViewModel>("SelectImageThemeColorDialog");
dialogs.WithDialog<Views.Dialogs.CreateLaunchScriptDialog, ViewModels.Dialogs.CreateLaunchScriptDialogViewModel>("CreateLaunchScriptDialog");
dialogs.WithDialog<Views.Dialogs.ImportModpackDialog, ViewModels.Dialogs.ImportModpackDialogViewModel>("ImportModpackDialog");
dialogs.WithDialog<Views.Dialogs.InputInstanceIdDialog, ViewModels.Dialogs.InputInstanceIdDialogViewModel>("InputInstanceIdDialog");

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

// Services
services.AddSingleton<GameService>();
services.AddSingleton<LaunchService>();
services.AddSingleton<AccountService>();
services.AddSingleton<AuthenticationService>();
services.AddSingleton<DownloadService>();
services.AddSingleton<LocalStorageService>();
services.AddSingleton<MessengerService>();
services.AddSingleton<AppearanceService>();
services.AddSingleton<CacheInterfaceService>();
services.AddSingleton<QuickLaunchService>();
services.AddSingleton<SearchProviderService>();
services.AddSingleton<InstanceConfigService>();

// Notification Services
services.AddSingleton<INotificationService, NotificationService>();

services.AddSingleton<InfoBarPresenter>();
services.AddSingleton<SystemNotificationPresenter>();
services.AddSingleton<TeachingTipPresenter>();

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
services.AddSingleton<UpdateService>();
#endif

#endregion

// Build and run the app
var app = builder.Build();
AppHost = app.Host;

HandleUriCommandParameters(ref args);
await BuildRootCommand(app).InvokeAsync(args);

internal partial class Program
{
    private static ILogger<Program> Logger => AppHost.Services.GetRequiredService<ILogger<Program>>();

    public static IHost AppHost { get; private set; } = null!;

    public static Option<string> MinecraftFolderOption { get; } = new(name: "--minecraftFolder") { IsRequired = true };

    public static Option<string> InstanceIdOption { get; } = new(name: "--instanceId") { IsRequired = true };

    public static RootCommand BuildRootCommand(WinUIApplication<App> application)
    {
        RootCommand rootCommand = [BuildSubCommand()];

        rootCommand.SetHandler(async () =>
        {
            Logger.Starting();
            await application.RunAsync();
        });

        return rootCommand;
    }

    public static Command BuildSubCommand()
    {
        var quickLaunchCommand = new Command("quickLaunch");
        quickLaunchCommand.AddOption(MinecraftFolderOption);
        quickLaunchCommand.AddOption(InstanceIdOption);
        quickLaunchCommand.AddAlias("quicklaunch");

        quickLaunchCommand.SetHandler(async (folder, instanceId) =>
            await AppHost.Services.GetService<QuickLaunchService>()!.LaunchFromArguments(folder, instanceId),
            MinecraftFolderOption, InstanceIdOption);

        return quickLaunchCommand;
    }

    public static void HandleUriCommandParameters(ref string[] args)
    {
        if (args.Length != 1 || !args[0].StartsWith("fluent-launcher://"))
            return;

        Uri requestUri = new(args[0]);
        var collection = HttpUtility.ParseQueryString(requestUri.Query);

        List<string> handledArgs = [requestUri.Host];

        foreach (var key in collection.Keys.OfType<string>())
        {
            handledArgs.Add($"--{key}");
            handledArgs.Add(collection[key]!);
        }

        args = [.. handledArgs];
    }
}

internal static partial class ProgramLoggers
{
    [LoggerMessage(LogLevel.Information, "Starting WinUIApplication.RunAsync ...")]
    public static partial void Starting(this ILogger logger);
}

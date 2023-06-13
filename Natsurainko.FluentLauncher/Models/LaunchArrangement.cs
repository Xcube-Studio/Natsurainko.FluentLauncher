using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Extension;
using Natsurainko.FluentCore.Extension.Windows.Extension;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentCore.Wrapper;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.CrossProcess;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Natsurainko.Toolkits.Network.Downloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinUIEx;
using GameCore = Natsurainko.FluentLauncher.Components.FluentCore.GameCore;
using GameCoreLocator = Natsurainko.FluentLauncher.Components.FluentCore.GameCoreLocator;

namespace Natsurainko.FluentLauncher.Models;

internal partial class LaunchArrangement : ObservableObject
{
    private static SettingsService _settings = App.GetService<SettingsService>();
    private static AccountService _accountService = App.GetService<AccountService>();

    public LaunchArrangement(GameCore core)
    {
        LaunchSetting = core.GetLaunchSetting();
        GameCore = core;
    }

    [ObservableProperty]
    private string state;

    [ObservableProperty]
    private string arguments;

    [ObservableProperty]
    private string elapsedTime;

    [ObservableProperty]
    private int exitCode;

    [ObservableProperty]
    private string exitDescription;

    [ObservableProperty]
    private Exception exception;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    private Visibility gameRunning = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility gameExied = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility gameLogger = Visibility.Collapsed;

    public GameCore GameCore { get; private set; }

    public LaunchSetting LaunchSetting { get; private set; }

    public LaunchResponse LaunchResponse { get; private set; }

    public List<GameProcessOutput> ProcessOutputs { get; private set; } = new();

    [RelayCommand]
    public void Kill() => LaunchResponse.Stop();

    [RelayCommand]
    public void Copy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(string.Join(' ', Arguments));
        Clipboard.SetContent(dataPackage);

        MessageService.ShowSuccess("Copied Arguments");
    }

    [RelayCommand]
    public void Logger()
    {
        var window = new WindowEx();

        window.Title = $"Logger - {GameCore.Id}";

#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        window.AppWindow.SetIcon(Path.Combine(Directory.GetCurrentDirectory(), "Assets/AppIcon.ico"));
#else 
        window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
#endif

        window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        window.SystemBackdrop = Environment.OSVersion.Version.Build >= 22000
           ? new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
           : new DesktopAcrylicBackdrop();

        (window.MinWidth, window.MinHeight) = (516, 328);
        (window.Width, window.Height) = (873, 612);

        var view = new Views.LoggerPage();
        var viewModel = new ViewModels.Pages.LoggerViewModel(ProcessOutputs, LaunchResponse, view);
        viewModel.Title = window.Title;
        view.DataContext = viewModel;

        window.Content = view;
        window.Show();
    }

    private void ReportState(string state)
        => App.MainWindow.DispatcherQueue.TryEnqueue(() => State = state);

    public static void StartNew(GameCore core)
    {
        var arrangement = new LaunchArrangement(core);
        LaunchResponse launchResponse = default;

        Task.Run(async () =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Launch Tasks" };
                hyperlinkButton.Click += (_, _) => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.LaunchPage));

                MessageService.Show(
                    $"Added Launch \"{core.Id}\" into Arrangements",
                    "Go to Activities>Launch Tasks for details",
                    button: hyperlinkButton);

                GlobalActivitiesCache.LaunchArrangements.Insert(0, arrangement);

                Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.LaunchPage));
            });

            var coreLocator = new GameCoreLocator(_settings.CurrentGameFolder);
            var launcher = new MinecraftLauncher(arrangement.LaunchSetting, coreLocator);
            var resourceDownloader = new CrossProcessResourceDownloader(App.GetService<SettingsService>());

            resourceDownloader.DownloadProgressChanged += (object sender, ParallelDownloaderProgressChangedEventArgs e)
                => arrangement.ReportState($"Downloading Assets {e.CompletedTasks}/{e.TotleTasks}");

            launcher.ResourceDownloader = resourceDownloader;

            if (_settings.AutoRefresh)
            {
                if (arrangement.LaunchSetting.Account.Type.Equals(AccountType.Microsoft))
                {
                    var account = (MicrosoftAccount)arrangement.LaunchSetting.Account;
                    launcher.Authenticator = new MicrosoftAuthenticator(
                         account.RefreshToken,
                        "0844e754-1d2e-4861-8e2b-18059609badb", 
                        "https://login.live.com/oauth20_desktop.srf",
                        AuthenticatorMethod.Refresh);
                }
                else if (arrangement.LaunchSetting.Account.Type.Equals(AccountType.Yggdrasil))
                {
                    var account = (YggdrasilAccount)arrangement.LaunchSetting.Account;
                    launcher.Authenticator = new YggdrasilAuthenticator(
                        AuthenticatorMethod.Refresh,
                        account.AccessToken,
                        account.ClientToken,
                        yggdrasilServerUrl: account.YggdrasilServerUrl);
                }
                else if (arrangement.LaunchSetting.Account.Type.Equals(AccountType.Offline))
                    launcher.Authenticator = new OfflineAuthenticator(arrangement.LaunchSetting.Account.Name, arrangement.LaunchSetting.Account.Uuid);
            }

            if (arrangement.LaunchSetting.Account.Type.Equals(AccountType.Yggdrasil))
            {
#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
                var authlibPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Libs", "authlib-injector-1.2.1.jar");
#else
                var authlibPath = (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Libs/authlib-injector-1.2.1.jar"))).Path;
#endif
                await foreach (var args in ((YggdrasilAccount)arrangement.LaunchSetting.Account).GetAuthlibArgumentsAsync(authlibPath))
                    arrangement.LaunchSetting.JvmSetting.JvmArguments.Add(args);
            }

            await Task.Run(() =>
            {
                launchResponse = launcher.LaunchMinecraft(core, args => arrangement.ReportState(LanguageResources.HandleLaunchState(args.Message)));
                arrangement.LaunchResponse = launchResponse;

                var arguemnts = launchResponse.Arguemnts?.Select(x => x.StartsWith("--accessToken") ? x.Split(' ')[0] + " " + new string('*', x.Split(' ')[1].Length) : x);

                if (launchResponse.State == LaunchState.Succeess)
                {
                    var timer = new System.Timers.Timer(1000);
                    timer.Elapsed += (_, e) => App.MainWindow.DispatcherQueue.TryEnqueue(() => arrangement.ElapsedTime = launchResponse.RunTime.Elapsed.ToString("hh\\:mm\\:ss"));
                    timer.Start();

                    void GameProcessOutput(object sender, GameProcessOutputArgs e)
                        => arrangement.ProcessOutputs.Add(e.GameProcessOutput);

                    launchResponse.GameExited += (object sender, GameExitedArgs e) =>
                    {
                        arrangement.ReportState($"Game Exited");

                        App.MainWindow.DispatcherQueue.SynchronousTryEnqueue(() =>
                        {
                            arrangement.ExitCode = e.ExitCode;
                            arrangement.GameRunning = Visibility.Collapsed;
                            arrangement.GameExied = Visibility.Visible;
                            arrangement.ExitDescription = e.ExitCode == 0
                                ? "Game Exited Normally"
                                : "Game Crashed";
                        });

                        timer.Stop();
                        timer.Dispose();

                        launchResponse.GameProcessOutput -= GameProcessOutput;
                        launchResponse.Dispose();
                    };
                    launchResponse.GameProcessOutput += GameProcessOutput;

                    arrangement.ReportState($"Game Running");

                    if (launcher.Authenticator != null)
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            _accountService.Remove(_accountService.ActiveAccount);

#pragma warning disable CS0612 // Type or member is obsolete
                            _accountService.AddAccount(arrangement.LaunchSetting.Account);
#pragma warning restore CS0612 // Type or member is obsolete
                            _accountService.Activate(arrangement.LaunchSetting.Account);
                        });
                    }

                    if (!string.IsNullOrEmpty(arrangement.LaunchSetting.GameWindowSetting.WindowTitle))
                        launchResponse.SetMainWindowTitle(arrangement.LaunchSetting.GameWindowSetting.WindowTitle);

                    core.CoreProfile.LastLaunchTime = DateTime.Now;

                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        arrangement.GameRunning = arrangement.GameLogger = Visibility.Visible;
                        arrangement.Arguments = string.Join("\r\n", arguemnts);
                    });
                }
                else
                {
                    arrangement.ReportState($"Launch Failed");
                    MessageService.ShowException(launchResponse.Exception, $"Failed to Launch \"{core.Id}\"");

                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        arrangement.Exception = launchResponse.Exception;
                        arrangement.Arguments = arguemnts != null ? string.Join("\r\n", arguemnts) : null;

                        await Task.Delay(TimeSpan.FromSeconds(2));
                        launchResponse.Dispose();
                    });
                }
            });
        }).ContinueWith(task =>
        {
            if (!task.IsFaulted)
                return;

            MessageService.ShowException(task.Exception, $"Failed to Launch \"{core.Id}\"");
            arrangement.ReportState($"Launch Failed");

            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                arrangement.Exception = task.Exception;
                arrangement.Arguments = null;

                await Task.Delay(TimeSpan.FromSeconds(2));
                launchResponse?.Dispose();
            });
        });
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Extension;
using Natsurainko.FluentCore.Extension.Windows.Extension;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentCore.Wrapper;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.CrossProcess;
using Natsurainko.FluentLauncher.Views.Pages;
using Natsurainko.Toolkits.Network.Downloader;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;
using GameCore = Natsurainko.FluentLauncher.Components.FluentCore.GameCore;
using GameCoreLocator = Natsurainko.FluentLauncher.Components.FluentCore.GameCoreLocator;

namespace Natsurainko.FluentLauncher.Models;

public partial class LaunchArrangement : ObservableObject
{
    public LaunchArrangement(GameCore core)
    {
        this.LaunchSetting = core.GetLaunchSetting();
        this.GameCore = core;
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

        MainContainer.ShowMessagesAsync("Copied Arguments", severity: Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
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
        window.Backdrop = Environment.OSVersion.Version.Build >= 22000
           ? new MicaSystemBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
           : new AcrylicSystemBackdrop()
           {
               DarkTintColor = Colors.Black,
               DarkFallbackColor = Colors.Black
           };

        (window.MinWidth, window.MinHeight) = (516, 328);
        (window.Width, window.Height) = (873, 612);

        var view = new Logger();
        var viewModel = new ViewModels.Pages.Logger(ProcessOutputs, LaunchResponse, view);
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
                hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Activities.Navigation), typeof(Views.Pages.Activities.Launch));

                MainContainer.ShowMessagesAsync(
                    $"Added Launch \"{core.Id}\" into Arrangements",
                    "Go to Activities>Launch Tasks for details",
                    button: hyperlinkButton);

                GlobalActivitiesCache.LaunchArrangements.Insert(0, arrangement);
            });

            var coreLocator = new GameCoreLocator(App.Configuration.CurrentGameFolder);
            var launcher = new MinecraftLauncher(arrangement.LaunchSetting, coreLocator);
            var resourceDownloader = new CrossProcessResourceDownloader();

            resourceDownloader.DownloadProgressChanged += (object sender, ParallelDownloaderProgressChangedEventArgs e)
                => arrangement.ReportState($"Downloading Assets {e.CompletedTasks}/{e.TotleTasks}");

            launcher.ResourceDownloader = resourceDownloader;

            if (App.Configuration.AutoRefresh)
            {
                if (arrangement.LaunchSetting.Account.Type.Equals(AccountType.Microsoft))
                {
                    var account = (MicrosoftAccount)arrangement.LaunchSetting.Account;
                    launcher.Authenticator = new MicrosoftAuthenticator(
                        account.RefreshToken,
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
                var authlibPath = Path.Combine(App.StoragePath, "Libraries", "authlib-injector-1.2.1.jar");

                if (!File.Exists(authlibPath))
                {
                    if (!Directory.Exists(Path.Combine(App.StoragePath, "Libraries")))
                        Directory.CreateDirectory(Path.Combine(App.StoragePath, "Libraries"));

                    File.Copy(Path.Combine(Package.Current.InstalledPath, "Natsurainko.FluentLauncher", "Assets", "Libs", "authlib-injector-1.2.1.jar"), authlibPath);
                }
#endif
                await foreach (var args in ((YggdrasilAccount)arrangement.LaunchSetting.Account).GetAuthlibArgumentsAsync(authlibPath))
                    arrangement.LaunchSetting.JvmSetting.AdvancedArguments.Add(args);
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
                        App.Configuration.Accounts.Remove(App.Configuration.CurrentAccount);

                        App.Configuration.Accounts.Add(arrangement.LaunchSetting.Account);
                        App.Configuration.CurrentAccount = arrangement.LaunchSetting.Account;

                        App.Configuration.ReportPropertyChanged(new(nameof(App.Configuration.Accounts)));
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
                    MainContainer.ShowMessagesAsync(
                        launchResponse.Exception.ToString(),
                        $"Failed to Launch \"{core.Id}\"",
                        severity: InfoBarSeverity.Error,
                        delay: 1000 * 20);

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

            arrangement.ReportState($"Launch Failed");
            MainContainer.ShowMessagesAsync(
                task.Exception.ToString(),
                $"Failed to Launch \"{core.Id}\"",
                severity: InfoBarSeverity.Error,
                delay: 1000 * 20);

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

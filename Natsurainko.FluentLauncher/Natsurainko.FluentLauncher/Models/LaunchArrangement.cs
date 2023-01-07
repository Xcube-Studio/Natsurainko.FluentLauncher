using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Downloader;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentCore.Wrapper;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Views.Pages;
using Natsurainko.Toolkits.Network.Downloader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Models;

public partial class LaunchArrangement : ObservableObject
{
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

        window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
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
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Launch Tasks" };
            hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Activities.Navigation), typeof(Views.Pages.Activities.Launch));

            MainContainer.ShowMessagesAsync(
                $"Added Launch \"{core.Id}\" into Arrangements",
                "Go to Activities>Launch Tasks for details",
                button: hyperlinkButton);
        });

        var arrangement = new LaunchArrangement
        {
            LaunchSetting = new()
            {
                Account = App.Configuration.CurrentAccount,
                JvmSetting = new()
                {
                    Javaw = new FileInfo(App.Configuration.CurrentJavaRuntime),
                    MinMemory = App.Configuration.JavaVirtualMachineMemory,
                }
            },
            GameCore = core
        };

        var coreLocator = new GameCoreLocator(App.Configuration.CurrentGameFolder);
        var launcher = new MinecraftLauncher(arrangement.LaunchSetting, coreLocator);
        var resourceDownloader = new ResourceDownloader();

        resourceDownloader.DownloadProgressChanged += (object sender, ParallelDownloaderProgressChangedEventArgs e)
            => arrangement.ReportState($"Downloading Assets {e.CompletedTasks}/{e.TotleTasks}");

        launcher.ResourceDownloader = resourceDownloader;

        var thread = new Thread(() =>
        {
            var launchResponse = launcher.LaunchMinecraft(core, args => arrangement.ReportState(LanguageStringHandler.HandleLaunchState(args.Message)));
            arrangement.LaunchResponse = launchResponse;

            if (launchResponse.State == LaunchState.Succeess)
            {
                void GameProcessOutput(object sender, GameProcessOutputArgs e)
                    => arrangement.ProcessOutputs.Add(e.GameProcessOutput);

                launchResponse.GameProcessOutput += GameProcessOutput;

                arrangement.ReportState($"Game Running");
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    arrangement.GameRunning = arrangement.GameLogger = Visibility.Visible;
                    arrangement.Arguments = string.Join(" ", launchResponse.Arguemnts);
                });

                var timer = new System.Timers.Timer(1000);
                timer.Elapsed += (_,e) => App.MainWindow.DispatcherQueue.TryEnqueue(() => arrangement.ElapsedTime = launchResponse.RunTime.Elapsed.ToString("hh\\:mm\\:ss"));
                timer.Start();

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
            }
            else
            {
                arrangement.ReportState($"Launch Failed");
                MainContainer.ShowMessagesAsync(
                    launchResponse.Exception.ToString(),
                    $"Failed to Launch \"{core.Id}\"",
                    severity: InfoBarSeverity.Error,
                    delay:1000 * 20);

                App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    arrangement.Exception = launchResponse.Exception;
                    arrangement.Arguments = string.Join(" ", launchResponse.Arguemnts);

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    launchResponse.Dispose();
                });
            }
        });
        thread.Start();

        GlobalActivitiesCache.LaunchArrangements.Insert(0, arrangement);
    }
}

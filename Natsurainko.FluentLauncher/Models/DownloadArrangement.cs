using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Install;
using Natsurainko.FluentCore.Model.Install.Fabric;
using Natsurainko.FluentCore.Model.Install.Forge;
using Natsurainko.FluentCore.Model.Install.OptiFine;
using Natsurainko.FluentCore.Model.Install.Quilt;
using Natsurainko.FluentCore.Model.Install.Vanilla;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.Toolkits.Network.Downloader;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Natsurainko.FluentLauncher.Models;

public partial class DownloadArrangement : ObservableObject
{
    [ObservableProperty]
    public double totalProgress;

    [ObservableProperty]
    private string percentage = "0%";

    [ObservableProperty]
    private string state;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string taskName;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    private string timeSpan = "00:00";

    public void ReportState(string state)
        => App.MainWindow.DispatcherQueue.TryEnqueue(() => State = state);

    [RelayCommand]
    public void Loaded(object parameter) => OnLoaded(parameter);

    public virtual void OnLoaded(object parameter) { }

    public virtual void Start()
    {

    }
}

public partial class InstallArrangement : DownloadArrangement
{
    private SettingsService _settings;

    public InstallArrangement(SettingsService settings)
    {
        _settings = settings;
    }

    public InstallArrangement(CoreManifestItem coreManifestItem, string customName, bool enableCoreIndependent, SettingsService settings) : this(settings)
    {
        TaskName = "Install Arrangement";
        Title = CustomName = customName;

        CoreManifestItem = coreManifestItem;
        EnableCoreIndependent = enableCoreIndependent;

        var gameCoreLocator = new GameCoreLocator(_settings.CurrentGameFolder);
        GameCoreInstaller = new MinecraftVanlliaInstaller(gameCoreLocator, coreManifestItem, customName);

        installerStepProgresses.CollectionChanged += InstallerStepProgresses_CollectionChanged;
        GameCoreInstaller.ProgressChanged += GameCoreInstaller_ProgressChanged;
        ReportState("Not Started");
    }

    public InstallArrangement(IModLoaderInstallBuild installBuild, string customName, bool enableCoreIndependent, SettingsService settings) : this(settings)
    {
        TaskName = "Install Arrangement";
        Title = CustomName = customName;

        InstallBuild = installBuild;
        EnableCoreIndependent = enableCoreIndependent;

        var gameCoreLocator = new GameCoreLocator(_settings.CurrentGameFolder);

        GameCoreInstaller = installBuild.ModLoaderType switch
        {
            ModLoaderType.Forge => new MinecraftForgeInstaller(
                gameCoreLocator,
                (ForgeInstallBuild)installBuild,
                _settings.CurrentJavaRuntime,
                customId: customName),
            ModLoaderType.Fabric => new MinecraftFabricInstaller(
                gameCoreLocator,
                (FabricInstallBuild)installBuild,
                customName),
            ModLoaderType.OptiFine => new MinecraftOptiFineInstaller(
                gameCoreLocator,
                (OptiFineInstallBuild)installBuild,
                _settings.CurrentJavaRuntime,
                customId: customName),
            ModLoaderType.Quilt => new MinecraftQuiltInstaller(
                gameCoreLocator,
                (QuiltInstallBuild)installBuild,
                customName),
            _ => throw new Exception()
        };

        installerStepProgresses.CollectionChanged += InstallerStepProgresses_CollectionChanged;
        GameCoreInstaller.ProgressChanged += GameCoreInstaller_ProgressChanged;
        ReportState("Not Started");
    }

    public CoreManifestItem CoreManifestItem { get; private set; }

    public IModLoaderInstallBuild InstallBuild { get; private set; }

    public FluentCore.Module.Installer.BaseGameCoreInstaller GameCoreInstaller { get; private set; }

    public string CustomName { get; private set; }

    public bool EnableCoreIndependent { get; private set; }

    private Stopwatch Stopwatch = new Stopwatch();

    public override void OnLoaded(object parameter) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
        parameter.As<ContentPresenter, object>(e => e.sender.ContentTemplate = e.sender.Resources["InstallControlTemplate"] as DataTemplate);
    });

    private void GameCoreInstaller_ProgressChanged(object sender, GameCoreInstallerProgressChangedEventArgs e)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (e.StepsProgress.Count > InstallerStepProgresses.Count())
                for (int i = InstallerStepProgresses.Count(); i < e.StepsProgress.Count; i++)
                    InstallerStepProgresses.Add(new InstallerStepProgress(e.StepsProgress.Values.ToArray()[i], this));

            TotalProgress = e.TotleProgress;
        });
    }

    private void InstallerStepProgresses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => GridViewVisibility = installerStepProgresses.Any() ? Visibility.Visible : Visibility.Collapsed;

    public partial class InstallerStepProgress : ObservableObject
    {
        public InstallArrangement Parent;

        public InstallerStepProgress(GameCoreInstallerStepProgress stepProgress, InstallArrangement parent)
        {
            StepProgress = stepProgress;
            stepProgress.ProgressChanged += StepProgress_ProgressChanged;
            Parent = parent;
        }

        private void StepProgress_ProgressChanged(object sender, EventArgs e) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(StepProgress));
            Parent.ReportState(StepProgress.StepName);
        });

        [ObservableProperty]
        GameCoreInstallerStepProgress stepProgress;
    }

    [ObservableProperty]
    private ObservableCollection<InstallerStepProgress> installerStepProgresses = new();

    [ObservableProperty]
    private Visibility gridViewVisibility = Visibility.Collapsed;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(TotalProgress))
            Percentage = $"{TotalProgress * 100:0.0}%";
    }

    public override void Start()
    {
        Stopwatch.Start();

        var timer = new Timer(1000);
        timer.Elapsed += (_, e) => App.MainWindow.DispatcherQueue.TryEnqueue(() => TimeSpan = Stopwatch.Elapsed.ToString("hh\\:mm\\:ss"));
        timer.Start();

        Task.Run(async () =>
        {
            var installerResponse = await GameCoreInstaller.InstallAsync();
            App.MainWindow.DispatcherQueue.TryEnqueue(() => TimeSpan = installerResponse.Stopwatch.Elapsed.ToString("hh\\:mm\\:ss"));

            if (installerResponse.Success)
            {
                if (EnableCoreIndependent)
                {
                    var core = (installerResponse.GameCore as GameCore);

                    core.CoreProfile.EnableSpecialSetting = true;
                    core.CoreProfile.LaunchSetting = new()
                    {
                        EnableIndependencyCore = true
                    };
                }

                MessageService.ShowSuccess($"Installed Core {CustomName} Successfully");
                GameCoreInstaller.ProgressChanged -= GameCoreInstaller_ProgressChanged;

                ReportState("Installed Successfully");
            }
            else
            {
                ReportState("Failed");

                MessageService.ShowException(installerResponse.Exception, $"Failed to Install Core {CustomName}");
            }

            Stopwatch.Stop();
            timer.Stop();
            timer.Dispose();
        });
    }

    public static void StartNew(CoreManifestItem coreManifestItem, string customName, bool enableCoreIndependent) => Task.Run(() =>
    {
        var installArrangement = new InstallArrangement(coreManifestItem, customName, enableCoreIndependent, App.GetService<SettingsService>());

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Download Tasks" };
            hyperlinkButton.Click += (_, _) => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.DownloadPage));

            MessageService.Show(
                $"Added Install \"{customName}\" into Arrangements",
                "Go to Activities>Download Tasks for details",
                button: hyperlinkButton);

            GlobalActivitiesCache.DownloadArrangements.Insert(0, installArrangement);
        });

        installArrangement.Start();
    });

    public static void StartNew(IModLoaderInstallBuild modLoaderInstallBuild, string customName, bool enableCoreIndependent) => Task.Run(() =>
    {
        var installArrangement = new InstallArrangement(modLoaderInstallBuild, customName, enableCoreIndependent, App.GetService<SettingsService>());

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Download Tasks" };
            hyperlinkButton.Click += (_, _) => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.DownloadPage));

            MessageService.Show(
                $"Added Install \"{customName}\" into Arrangements",
                "Go to Activities>Download Tasks for details",
                button: hyperlinkButton);

            GlobalActivitiesCache.DownloadArrangements.Insert(0, installArrangement);
        });

        installArrangement.Start();
    });
}

public partial class ModDownloadArrangement : DownloadArrangement
{
    public ModDownloadArrangement(string fileName, Func<DownloadRequest> downloadRequest)
    {
        TaskName = "Download Arrangement";
        Title = FileName = fileName;
        GetDownloadRequest = downloadRequest;

        ReportState("Not Started");
    }

    public string FileName { get; private set; }

    public Func<DownloadRequest> GetDownloadRequest { get; private set; }

    private Stopwatch Stopwatch = new Stopwatch();

    public override void Start()
    {
        Stopwatch.Start();

        var timer = new Timer(1000);
        timer.Elapsed += (_, e) => App.MainWindow.DispatcherQueue.TryEnqueue(() => TimeSpan = Stopwatch.Elapsed.ToString("hh\\:mm\\:ss"));
        timer.Start();

        Task.Run(async () =>
        {
            ReportState("Downloading");

            var downloadRequest = GetDownloadRequest();

            using IDownloader<SimpleDownloaderResponse, SimpleDownloaderProgressChangedEventArgs> downloader
                = App.GetService<SettingsService>().EnableFragmentDownload && downloadRequest.FileSize.GetValueOrDefault(0L) >= 1572864L
                ? new FragmentDownloader(downloadRequest)
                : new SimpleDownloader(downloadRequest);

            downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;

            App.MainWindow.DispatcherQueue.TryEnqueue(() => TimeSpan = Stopwatch.Elapsed.ToString("hh\\:mm\\:ss"));

            downloader.BeginDownload();
            var response = await downloader.CompleteAsync();

            if (response.Success)
            {
                MessageService.ShowSuccess($"Downloaded Mod {FileName} Successfully");
                ReportState("Downloaded Successfully");
            }
            else
            {
                MessageService.ShowException(response.Exception, $"Failed to Download Mod {FileName}");
                ReportState("Failed");
            }

            Stopwatch.Stop();
            timer.Stop();
            timer.Dispose();
        });
    }

    private void Downloader_DownloadProgressChanged(object sender, SimpleDownloaderProgressChangedEventArgs e)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            TotalProgress = e.Progress;
            Percentage = $"{e.Progress * 100:0.0}%";
        });
    }

    public override void OnLoaded(object parameter) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
        parameter.As<ContentPresenter, object>(e => e.sender.ContentTemplate = e.sender.Resources["DownloadControlTemplate"] as DataTemplate);
    });

    public static void StartNew(string fileName, Func<DownloadRequest> downloadRequest) => Task.Run(() =>
    {
        var modDownloadArrangement = new ModDownloadArrangement(fileName, downloadRequest);

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Download Tasks" };
            hyperlinkButton.Click += (_, _) => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.DownloadPage));

            MessageService.Show(
                $"Added Download Mod \"{fileName}\" into Arrangements",
                "Go to Activities>Download Tasks for details",
                button: hyperlinkButton);

            GlobalActivitiesCache.DownloadArrangements.Insert(0, modDownloadArrangement);
        });

        modDownloadArrangement.Start();
    });
}
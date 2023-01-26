using Accessibility;
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
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Web.Http;

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
    public InstallArrangement(CoreManifestItem coreManifestItem, string customName, bool enableCoreIndependent)
    {
        TaskName = "Install Arrangement";
        Title = CustomName = customName;

        CoreManifestItem = coreManifestItem;
        EnableCoreIndependent = enableCoreIndependent;

        var gameCoreLocator = new GameCoreLocator(App.Configuration.CurrentGameFolder);
        GameCoreInstaller = new MinecraftVanlliaInstaller(gameCoreLocator, coreManifestItem, customName);

        installerStepProgresses.CollectionChanged += InstallerStepProgresses_CollectionChanged;
        GameCoreInstaller.ProgressChanged += GameCoreInstaller_ProgressChanged;
        ReportState("Not Started");
    }

    public InstallArrangement(IModLoaderInstallBuild installBuild, string customName, bool enableCoreIndependent)
    {
        TaskName = "Install Arrangement";
        Title = CustomName = customName;

        InstallBuild = installBuild;
        EnableCoreIndependent = enableCoreIndependent;

        var gameCoreLocator = new GameCoreLocator(App.Configuration.CurrentGameFolder);

        GameCoreInstaller = installBuild.ModLoaderType switch
        {
            ModLoaderType.Forge => new MinecraftForgeInstaller(
                gameCoreLocator, 
                (ForgeInstallBuild)installBuild,
                App.Configuration.CurrentJavaRuntime,
                customId:customName),
            ModLoaderType.Fabric => new MinecraftFabricInstaller(
                gameCoreLocator,
                (FabricInstallBuild)installBuild,
                customName),
            ModLoaderType.OptiFine => new MinecraftOptiFineInstaller(
                gameCoreLocator,
                (OptiFineInstallBuild)installBuild,
                App.Configuration.CurrentJavaRuntime,
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

                MainContainer.ShowMessagesAsync($"Installed Core {CustomName} Successfully", 
                    severity: InfoBarSeverity.Success);
                GameCoreInstaller.ProgressChanged -= GameCoreInstaller_ProgressChanged;

                ReportState("Installed Successfully");
            }
            else
            {
                ReportState("Failed");

                MainContainer.ShowMessagesAsync($"Failed to Install Core {CustomName}",
                    installerResponse.Exception.ToString(),
                    delay: 1000 * 15,
                    severity: InfoBarSeverity.Error);
            }

            Stopwatch.Stop();
            timer.Stop();
            timer.Dispose();
        });
    }

    public static void StartNew(CoreManifestItem coreManifestItem, string customName, bool enableCoreIndependent) => Task.Run(() =>
    {
        var installArrangement = new InstallArrangement(coreManifestItem, customName, enableCoreIndependent);

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Download Tasks" };
            hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Activities.Navigation), typeof(Views.Pages.Activities.Download));

            MainContainer.ShowMessagesAsync(
                $"Added Install \"{customName}\" into Arrangements",
                "Go to Activities>Download Tasks for details",
                button: hyperlinkButton);

            GlobalActivitiesCache.DownloadArrangements.Insert(0, installArrangement);
        });

        installArrangement.Start();
    });

    public static void StartNew(IModLoaderInstallBuild modLoaderInstallBuild, string customName, bool enableCoreIndependent) => Task.Run(() =>
    {
        var installArrangement = new InstallArrangement(modLoaderInstallBuild, customName, enableCoreIndependent);

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hyperlinkButton = new HyperlinkButton { Content = "Go to Activities>Download Tasks" };
            hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Activities.Navigation), typeof(Views.Pages.Activities.Download));

            MainContainer.ShowMessagesAsync(
                $"Added Install \"{customName}\" into Arrangements",
                "Go to Activities>Download Tasks for details",
                button: hyperlinkButton);

            GlobalActivitiesCache.DownloadArrangements.Insert(0, installArrangement);
        });

        installArrangement.Start();
    });
}
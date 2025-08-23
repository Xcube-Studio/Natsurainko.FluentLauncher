using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels;
using Nrk.FluentCore.Experimental.GameManagement.Installer.Modpack;
using Nrk.FluentCore.Experimental.GameManagement.Modpacks;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.Modpack.CurseForgeModpackInstaller;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.Modpack.ModrinthModpackInstaller;
using static Nrk.FluentCore.GameManagement.Installer.FabricInstanceInstaller;
using static Nrk.FluentCore.GameManagement.Installer.ForgeInstanceInstaller;
using static Nrk.FluentCore.GameManagement.Installer.OptiFineInstanceInstaller;
using static Nrk.FluentCore.GameManagement.Installer.QuiltInstanceInstaller;
using static Nrk.FluentCore.GameManagement.Installer.VanillaInstanceInstaller;

namespace Natsurainko.FluentLauncher.Services.Network;

internal partial class DownloadService
{
    private MultipartDownloader _downloader;

    private readonly SettingsService _settingsService;
    private readonly LocalStorageService _localStorageService;
    private readonly INotificationService _notificationService;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly HttpClient _httpClient;

    public ObservableCollection<TaskViewModel> DownloadTasks { get; } = [];

    public IDownloader Downloader { get => _downloader; }

    public IDownloadMirror? DownloadMirror => _settingsService.CurrentDownloadSource == "Bmclapi" ? DownloadMirrors.BmclApi : null;

    public int RunningTasks => DownloadTasks.Count(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared);

    public DownloadService(
        SettingsService settingsService,
        LocalStorageService localStorageService,
        INotificationService notificationService,
        CurseForgeClient curseForgeClient,
        HttpClient httpClient)
    {
        _settingsService = settingsService;
        _localStorageService = localStorageService;
        _notificationService = notificationService;
        _curseForgeClient = curseForgeClient;
        _httpClient = httpClient;

        SetDownloader();

        _settingsService.MaxDownloadThreadsChanged += (_,_) => SetDownloader();
        _settingsService.CurrentDownloadSourceChanged += (_, _) => SetDownloader();
    }

    public async Task DownloadResourceFileAsync(CurseForgeFile curseForgeFile, string folder, Action<string>? continueWith = null)
    {
        DownloadResourceTaskViewModel downloadResourceTask = new(this, curseForgeFile, folder, continueWith);

        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, downloadResourceTask));
        await downloadResourceTask.EnqueueAsync();
    }

    public async Task DownloadResourceFileAsync(ModrinthFile modrinthFile, string folder, Action<string>? continueWith = null)
    {
        DownloadResourceTaskViewModel downloadResourceTask = new(this, modrinthFile, folder, continueWith);

        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, downloadResourceTask));
        await downloadResourceTask.EnqueueAsync();
    }

    public async Task DownloadResourceFileAsync(string url, string filePath, Action<string>? continueWith = null)
    {
        DownloadResourceTaskViewModel downloadResourceTask = new(this, url, filePath, continueWith);

        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, downloadResourceTask));
        await downloadResourceTask.EnqueueAsync();
    }

    public async Task InstallInstanceAsync(InstanceInstallConfig config)
    {
        InstallInstanceTaskViewModel installInstanceTask = new(
            this,
            GetInstanceInstaller(config, out var installationStageViews),
            config,
            installationStageViews);

        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, installInstanceTask));
        await installInstanceTask.EnqueueAsync();
    }

    public async Task InstallModpackAsync(ModpackInstallConfig modpackInstallConfig)
    {
        string minecraftFolder = _settingsService.ActiveMinecraftFolder ?? throw new InvalidOperationException();
        string javaPath = _settingsService.ActiveJava;

        ObservableCollection<InstallationStageViewModel> installationStagesViewModel;

        IInstanceInstaller instanceInstaller = modpackInstallConfig.ModpackInfo.ModpackType switch
        {
            ModpackType.CurseForge => new CurseForgeModpackInstaller
            {
                ModpackFilePath = modpackInstallConfig.ModpackFilePath,
                CurseForgeClient = _curseForgeClient,
                Downloader = _downloader,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                Progress = GetModpackInstallationViewModel<CurseForgeModpackInstallationStage>(out var createProgressReporterDelegate, out installationStagesViewModel),
                CreateModLoderInstallerProgressReporter = createProgressReporterDelegate,
                JavaPath = javaPath,
                CustomizedInstanceId = modpackInstallConfig.InstanceId,
                DeletePackageAfterInstallation = modpackInstallConfig.DeleteModpackFileAfterInstall
            },
            ModpackType.Modrinth => new ModrinthModpackInstaller
            {
                ModpackFilePath = modpackInstallConfig.ModpackFilePath,
                Downloader = _downloader,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                Progress = GetModpackInstallationViewModel<ModrinthModpackInstallationStage>(out var createProgressReporterDelegate, out installationStagesViewModel),
                CreateModLoderInstallerProgressReporter = createProgressReporterDelegate,
                JavaPath = javaPath,
                CustomizedInstanceId = modpackInstallConfig.InstanceId,
                DeletePackageAfterInstallation = modpackInstallConfig.DeleteModpackFileAfterInstall
            },
            _ => throw new NotImplementedException()
        };

        InstallModpackTaskViewModel installModpackTaskViewModel = new
        (
            this, 
            instanceInstaller,
            modpackInstallConfig,
            installationStagesViewModel
        );

        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, installModpackTaskViewModel));
        await installModpackTaskViewModel.EnqueueAsync();
    }

    public async Task DownloadAndInstallModpackAsync(CurseForgeFile curseForgeFile, string instanceId)
    {
        var directoryInfo = _localStorageService.GetDirectory("cache-downloads");
        await DownloadResourceFileAsync(curseForgeFile, directoryInfo.FullName, f =>
        {
            if (ModpackInfoParser.TryParseModpack(f, out var modpackInfo))
            {
                InstallModpackAsync(new ModpackInstallConfig
                {
                    InstanceId = instanceId,
                    ModpackFilePath = f,
                    ModpackInfo = modpackInfo,
                    DeleteModpackFileAfterInstall = true
                }).Forget();
            }
            else _notificationService.ModpackParseFailed();
        });
    }

    public async Task DownloadAndInstallModpackAsync(ModrinthFile modrinthFile, string instanceId)
    {
        var directoryInfo = _localStorageService.GetDirectory("cache-downloads");
        await DownloadResourceFileAsync(modrinthFile, directoryInfo.FullName, f =>
        {
            if (ModpackInfoParser.TryParseModpack(f, out var modpackInfo))
            {
                InstallModpackAsync(new ModpackInstallConfig
                {
                    InstanceId = instanceId,
                    ModpackFilePath = f,
                    ModpackInfo = modpackInfo,
                    DeleteModpackFileAfterInstall = true
                }).Forget();
            }
            else _notificationService.ModpackParseFailed();
        });
    }

    [MemberNotNull(nameof(_downloader))]
    private void SetDownloader()
    {
        _downloader = new(
            httpClient: _httpClient,
            workersPerDownloadTask: 8,
            concurrentDownloadTasks: _settingsService.MaxDownloadThreads,
            enableMultiPartDownload: _settingsService.EnableFragmentDownload,
            mirror: DownloadMirror);
    }

    IInstanceInstaller GetInstanceInstaller(
        InstanceInstallConfig instanceInstallConfig,
        out ObservableCollection<InstallationStageViewModel> installationStageViews)
    {
        var versionManifestItem = instanceInstallConfig.ManifestItem;
        string minecraftFolder = _settingsService.ActiveMinecraftFolder ?? throw new InvalidOperationException();
        string javaPath = _settingsService.ActiveJava;
        string customizedInstanceId = instanceInstallConfig.InstanceId ?? throw new InvalidOperationException();

        if (instanceInstallConfig.PrimaryLoader == null)
        {
            InstallationViewModel<VanillaInstallationStage> installationViewModel = new();
            installationStageViews = [.. installationViewModel.Stages.Values];

            return new VanillaInstanceInstaller
            {
                CheckAllDependencies = true,
                Downloader = _downloader,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                Progress = installationViewModel,
                CustomizedInstanceId = customizedInstanceId,
            };
        }

        ModLoaderType modLoaderType = instanceInstallConfig.PrimaryLoader.Type;
        object selectedInstallData = instanceInstallConfig.PrimaryLoader.SelectedInstallData;

        return modLoaderType switch
        {
            ModLoaderType.Forge => new ForgeInstanceInstaller()
            {
                Downloader = _downloader,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (ForgeInstallData)selectedInstallData,
                Progress = GetInstallationViewModel<ForgeInstallationStage>(out var vanillaStagesViewModel, out installationStageViews),
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                IsNeoForgeInstaller = false,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.NeoForge => new ForgeInstanceInstaller()
            {
                Downloader = _downloader,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (ForgeInstallData)selectedInstallData,
                Progress = GetInstallationViewModel<ForgeInstallationStage>(out var vanillaStagesViewModel, out installationStageViews),
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                IsNeoForgeInstaller = true,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.OptiFine => new OptiFineInstanceInstaller()
            {
                Downloader = _downloader,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (OptiFineInstallData)selectedInstallData,
                Progress = GetInstallationViewModel<OptiFineInstallationStage>(out var vanillaStagesViewModel, out installationStageViews),
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.Fabric => new FabricInstanceInstaller()
            {
                Downloader = _downloader,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (FabricInstallData)selectedInstallData,
                Progress = GetInstallationViewModel<FabricInstallationStage>(out var vanillaStagesViewModel, out installationStageViews),
                VanillaInstallationProgress = vanillaStagesViewModel,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.Quilt => new QuiltInstanceInstaller()
            {
                Downloader = _downloader,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (QuiltInstallData)selectedInstallData,
                Progress = GetInstallationViewModel<QuiltInstallationStage>(out var vanillaStagesViewModel, out installationStageViews),
                VanillaInstallationProgress = vanillaStagesViewModel,
                CustomizedInstanceId = customizedInstanceId
            },
            _ => throw new NotImplementedException()
        };
    }

    static InstallationViewModel<TStage> GetInstallationViewModel<TStage>(
        out InstallationViewModel<VanillaInstallationStage> vanillaStagesViewModel,
        out ObservableCollection<InstallationStageViewModel> installationStagesViewModel) where TStage : struct, Enum
    {
        ObservableCollection<InstallationStageViewModel> stageViewModels = [];

        InstallationViewModel<TStage> installationViewModel = new();
        installationViewModel.StageSkipped += (_, stage) =>
            App.DispatcherQueue.EnqueueAsync(() => stageViewModels.Remove(stage)).Wait();

        vanillaStagesViewModel = new();
        vanillaStagesViewModel.StageSkipped += (_, stage) =>
            App.DispatcherQueue.EnqueueAsync(() => stageViewModels.Remove(stage)).Wait();

        foreach (var item in installationViewModel.Stages.Values)
            stageViewModels.Add(item);

        foreach (var item in vanillaStagesViewModel.Stages.Values.Reverse())
            stageViewModels.Insert(1, item);

        installationStagesViewModel = stageViewModels;
        return installationViewModel;
    }

    static InstallationViewModel<TStage> GetModpackInstallationViewModel<TStage>(
        out CreateModLoderInstallerProgressReporterDelegate createProgressReporterDelegate,
        out ObservableCollection<InstallationStageViewModel> installationStagesViewModel) where TStage : struct, Enum
    {
        InstallationViewModel<TStage> installationViewModel = new();

        ObservableCollection<InstallationStageViewModel> stageViewModels = [];

        foreach (var item in installationViewModel.Stages.Values)
            stageViewModels.Add(item);

        installationViewModel.StageSkipped += (_, stage) =>
            App.DispatcherQueue.EnqueueAsync(() => stageViewModels.Remove(stage)).Wait();
        installationStagesViewModel = stageViewModels;

        createProgressReporterDelegate = (modLoaderType, out vanillaInstallationProgress) =>
        {
            IProgress<IInstallerProgress> progress;
            InstallationViewModel<VanillaInstallationStage> vanillaStagesViewModel;
            ObservableCollection<InstallationStageViewModel> subStageViewModels;

            if (modLoaderType == ModLoaderType.Forge || modLoaderType == ModLoaderType.NeoForge)
            {
                progress = GetInstallationViewModel<ForgeInstallationStage>(out vanillaStagesViewModel, out subStageViewModels);
                vanillaInstallationProgress = vanillaStagesViewModel;
            }
            else if (modLoaderType == ModLoaderType.Fabric)
            {
                progress = GetInstallationViewModel<FabricInstallationStage>(out vanillaStagesViewModel, out subStageViewModels);
                vanillaInstallationProgress = vanillaStagesViewModel;
            }
            else if (modLoaderType == ModLoaderType.Quilt)
            {
                progress = GetInstallationViewModel<QuiltInstallationStage>(out vanillaStagesViewModel, out subStageViewModels);
                vanillaInstallationProgress = vanillaStagesViewModel;
            }
            else throw new NotImplementedException();

            subStageViewModels.CollectionChanged += (sender, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    App.DispatcherQueue.EnqueueAsync(() =>
                    {
                        foreach (var item in e.OldItems!)
                            stageViewModels.Remove((InstallationStageViewModel)item);
                    }).Wait();
            };

            App.DispatcherQueue.EnqueueAsync(() =>
            {
                foreach (var item in subStageViewModels.Reverse())
                    stageViewModels.Insert(3, item);
            }).Wait();

            return progress;
        };


        return installationViewModel;
    }
}

internal static partial class DownloadServiceNotifications
{
    [Notification<InfoBar>(Title = "Notifications__ModpackParseFailed", Type = NotificationType.Error)]
    public static partial void ModpackParseFailed(this INotificationService notificationService);
}
using CommunityToolkit.WinUI;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Settings;
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
    private readonly HttpClient _httpClient;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public ObservableCollection<TaskViewModel> DownloadTasks { get; } = [];

    public IDownloader Downloader { get => _downloader; }

    public IDownloadMirror? DownloadMirror => _settingsService.CurrentDownloadSource == "Bmclapi" ? DownloadMirrors.BmclApi : null;

    public int RunningTasks => DownloadTasks.Count(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared);

    public DownloadService(
        SettingsService settingsService,
        CurseForgeClient curseForgeClient,
        ModrinthClient modrinthClient,
        HttpClient httpClient)
    {
        _settingsService = settingsService;
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
        _httpClient = httpClient;

        SetDownloader();

        _settingsService.MaxDownloadThreadsChanged += (_,_) => SetDownloader();
        _settingsService.CurrentDownloadSourceChanged += (_, _) => SetDownloader();
    }

    public async Task DownloadModFileAsync(CurseForgeFile curseForgeFile, string folder)
    {
        DownloadModTaskViewModel downloadModTask = new(this, curseForgeFile, folder);
        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, downloadModTask));
        await downloadModTask.EnqueueAsync();
    }

    public async Task DownloadModFileAsync(ModrinthFile modrinthFile, string folder)
    {
        DownloadModTaskViewModel downloadModTask = new(this, modrinthFile, folder);
        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, downloadModTask));
        await downloadModTask.EnqueueAsync();
    }

    public async Task DownloadModFileAsync(string url, string filePath)
    {
        DownloadModTaskViewModel downloadModTask = new(this, url, filePath);
        await App.DispatcherQueue.EnqueueAsync(() => DownloadTasks.Insert(0, downloadModTask));
        await downloadModTask.EnqueueAsync();
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

        RangeObservableCollection<InstallationStageViewModel> installationStagesViewModel;

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
                JavaPath = javaPath
            },
            ModpackType.Modrinth => new ModrinthModpackInstaller
            {
                ModpackFilePath = modpackInstallConfig.ModpackFilePath,
                CurseForgeClient = _curseForgeClient,
                Downloader = _downloader,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                Progress = GetModpackInstallationViewModel<CurseForgeModpackInstallationStage>(out var createProgressReporterDelegate, out installationStagesViewModel),
                CreateModLoderInstallerProgressReporter = createProgressReporterDelegate,
                JavaPath = javaPath
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
        out RangeObservableCollection<InstallationStageViewModel> installationStageViews)
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
                Progress = installationViewModel
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
        out RangeObservableCollection<InstallationStageViewModel> installationStagesViewModel) where TStage : struct, Enum
    {
        RangeObservableCollection<InstallationStageViewModel> stageViewModels = [];

        InstallationViewModel<TStage> installationViewModel = new();
        vanillaStagesViewModel = new();
        vanillaStagesViewModel.StageSkipped += (_, stage) =>
            App.DispatcherQueue.TryEnqueue(() => stageViewModels.Remove(stage));

        stageViewModels.AddRange(installationViewModel.Stages.Values);
        stageViewModels.InsertRange(1, vanillaStagesViewModel.Stages.Values);

        installationStagesViewModel = stageViewModels;
        return installationViewModel;
    }

    static InstallationViewModel<TStage> GetModpackInstallationViewModel<TStage>(
        out CreateModLoderInstallerProgressReporterDelegate createProgressReporterDelegate,
        out RangeObservableCollection<InstallationStageViewModel> installationStagesViewModel) where TStage : struct, Enum
    {
        InstallationViewModel<TStage> installationViewModel = new();

        RangeObservableCollection<InstallationStageViewModel> stageViewModels = [];
        stageViewModels.AddRange(installationViewModel.Stages.Values);

        installationStagesViewModel = stageViewModels;

        createProgressReporterDelegate = (modLoaderType, out vanillaInstallationProgress) =>
        {
            IProgress<IInstallerProgress> progress;
            InstallationViewModel<VanillaInstallationStage> vanillaStagesViewModel;
            RangeObservableCollection<InstallationStageViewModel> subStageViewModels;

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

            vanillaStagesViewModel.StageSkipped += (_, stage) =>
                App.DispatcherQueue.TryEnqueue(() => stageViewModels.Remove(stage));

            App.DispatcherQueue.TryEnqueue(() => stageViewModels.InsertRange(3, subStageViewModels));

            return progress;
        };


        return installationViewModel;
    }
}

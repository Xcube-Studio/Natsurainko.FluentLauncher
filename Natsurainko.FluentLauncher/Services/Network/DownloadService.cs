using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
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

    public ObservableCollection<TaskViewModel> DownloadTasks { get; } = [];

    public IDownloader Downloader { get => _downloader; }

    public IDownloadMirror? DownloadMirror => _settingsService.CurrentDownloadSource == "Bmclapi" ? DownloadMirrors.BmclApi : null;

    public int RunningTasks => DownloadTasks.Count(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared);

    public DownloadService(SettingsService settingsService, HttpClient httpClient)
    {
        _settingsService = settingsService;
        _httpClient = httpClient;

        SetDownloader();

        _settingsService.MaxDownloadThreadsChanged += (_,_) => SetDownloader();
        _settingsService.CurrentDownloadSourceChanged += (_, _) => SetDownloader();
    }

    public void DownloadModFile(object modFile, string folder)
    {
        DownloadModTaskViewModel downloadModTask = new(this, modFile, folder);
        App.DispatcherQueue.TryEnqueue(() => DownloadTasks.Insert(0, downloadModTask));
        downloadModTask.EnqueueAsync().Forget();
    }

    public void DownloadModFile(string url, string filePath)
    {
        DownloadModTaskViewModel downloadModTask = new(this, url, filePath);
        App.DispatcherQueue.TryEnqueue(() => DownloadTasks.Insert(0, downloadModTask));
        downloadModTask.EnqueueAsync().Forget();
    }

    public void InstallInstance(InstanceInstallConfig config)
    {
        InstallInstanceTaskViewModel installInstanceTask = new(
            this,
            GetInstanceInstaller(config, out var installationStageViews),
            config,
            installationStageViews);

        App.DispatcherQueue.TryEnqueue(() => DownloadTasks.Insert(0, installInstanceTask));
        installInstanceTask.EnqueueAsync().Forget();
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
        out List<InstallationStageViewModel> installationStageViews)
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
        out List<InstallationStageViewModel> installationStagesViewModel) where TStage : struct, Enum
    {
        InstallationViewModel<TStage> installationViewModel = new();
        vanillaStagesViewModel = new();

        installationStagesViewModel = [];
        installationStagesViewModel.AddRange(installationViewModel.Stages.Values);
        installationStagesViewModel.InsertRange(1, vanillaStagesViewModel.Stages.Values);

        return installationViewModel;
    }
}

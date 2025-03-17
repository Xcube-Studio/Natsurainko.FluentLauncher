using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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

    public event EventHandler? TaskListStateChanged;

    public ObservableCollection<TaskViewModel> DownloadTasks { get; } = [];

    public IDownloader Downloader { get => _downloader; }

    public DownloadService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        SetDownloader();

        _settingsService.MaxDownloadThreadsChanged += (_,_) => SetDownloader();
        _settingsService.CurrentDownloadSourceChanged += (_, _) => SetDownloader();
    }

    public void DownloadModFile(object modFile, string folder)
    {
        var taskViewModel = new DownloadModTaskViewModel(modFile, folder);
        taskViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "TaskState")
                TaskListStateChanged?.Invoke(this, e);
        };

        InsertTask(taskViewModel);
        taskViewModel.Start();
    }

    public void DownloadModFile(string fileName, string url, string folder)
    {
        var taskViewModel = new DownloadModTaskViewModel(fileName, url, folder);
        taskViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "TaskState")
                TaskListStateChanged?.Invoke(this, e);
        };

        InsertTask(taskViewModel);
        taskViewModel.Start();
    }

    public void InstallInstance(InstanceInstallConfig config)
    {
        var taskViewModel = new InstallInstanceTaskViewModel(
            GetInstanceInstaller(config, out var installationStageViews),
            config,
            installationStageViews);
        taskViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "TaskState")
                TaskListStateChanged?.Invoke(this, e);
        };

        InsertTask(taskViewModel);
        taskViewModel.Start();
    }

    private void InsertTask(TaskViewModel taskViewModel)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            DownloadTasks.Insert(0, taskViewModel);
            TaskListStateChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    [MemberNotNull(nameof(_downloader))]
    private void SetDownloader()
    {
        _downloader = new(
            httpClient: HttpUtils.HttpClient,
            workersPerDownloadTask: 8,
            concurrentDownloadTasks: _settingsService.MaxDownloadThreads,
            enableMultiPartDownload: _settingsService.EnableFragmentDownload,
            mirror: _settingsService.CurrentDownloadSource == "Bmclapi" ? DownloadMirrors.BmclApi : null);
    }

    IInstanceInstaller GetInstanceInstaller(
        InstanceInstallConfig instanceInstallConfig,
        out IReadOnlyList<InstallationStageViewModel> installationStageViews)
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
                DownloadMirror = DownloadMirrors.BmclApi,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                Progress = installationViewModel
            };
        }

        ModLoaderType modLoaderType = instanceInstallConfig.PrimaryLoader.Type;
        object selectedInstallData = instanceInstallConfig.PrimaryLoader.SelectedInstallData;
        IDownloadMirror? downloadMirror = _settingsService.CurrentDownloadSource == "Bmclapi" ? DownloadMirrors.BmclApi : null;

        installationStageViews = GetInstallationViewModel(modLoaderType, out var vanillaStagesViewModel, out var stagesViewModel);

        IInstanceInstaller installer = modLoaderType switch
        {
            ModLoaderType.Forge => new ForgeInstanceInstaller()
            {
                DownloadMirror = downloadMirror,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (ForgeInstallData)selectedInstallData,
                Progress = (InstallationViewModel<ForgeInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                IsNeoForgeInstaller = false,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.NeoForge => new ForgeInstanceInstaller()
            {
                DownloadMirror = downloadMirror,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (ForgeInstallData)selectedInstallData,
                Progress = (InstallationViewModel<ForgeInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                IsNeoForgeInstaller = true,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.OptiFine => new OptiFineInstanceInstaller()
            {
                DownloadMirror = downloadMirror,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (OptiFineInstallData)selectedInstallData,
                Progress = (InstallationViewModel<OptiFineInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.Fabric => new FabricInstanceInstaller()
            {
                DownloadMirror = downloadMirror,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (FabricInstallData)selectedInstallData,
                Progress = (InstallationViewModel<FabricInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                CustomizedInstanceId = customizedInstanceId
            },
            ModLoaderType.Quilt => new QuiltInstanceInstaller()
            {
                DownloadMirror = downloadMirror,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (QuiltInstallData)selectedInstallData,
                Progress = (InstallationViewModel<QuiltInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                CustomizedInstanceId = customizedInstanceId
            },
            _ => throw new InvalidOperationException()
        };

        return installer;
    }

    List<InstallationStageViewModel> GetInstallationViewModel(
        ModLoaderType modLoaderType,
        out InstallationViewModel<VanillaInstallationStage> vanillaStagesViewModel,
        out object stagesViewModel)
        {
            List<InstallationStageViewModel> stageViewModels = [];
            vanillaStagesViewModel = new();

            if (modLoaderType == ModLoaderType.Quilt)
            {
                InstallationViewModel<QuiltInstallationStage> installationViewModel = new();

                stageViewModels.AddRange(installationViewModel.Stages.Values);
                stageViewModels.InsertRange(1, vanillaStagesViewModel.Stages.Values);

                stagesViewModel = installationViewModel;
            }
            else if (modLoaderType == ModLoaderType.Fabric)
            {
                InstallationViewModel<FabricInstallationStage> installationViewModel = new();

                stageViewModels.AddRange(installationViewModel.Stages.Values);
                stageViewModels.InsertRange(1, vanillaStagesViewModel.Stages.Values);

                stagesViewModel = installationViewModel;
            }
            else if (modLoaderType == ModLoaderType.Forge || modLoaderType == ModLoaderType.NeoForge)
            {
                InstallationViewModel<ForgeInstallationStage> installationViewModel = new();

                stageViewModels.AddRange(installationViewModel.Stages.Values);
                stageViewModels.InsertRange(1, vanillaStagesViewModel.Stages.Values);

                stagesViewModel = installationViewModel;
            }
            else if (modLoaderType == ModLoaderType.OptiFine)
            {
                InstallationViewModel<OptiFineInstallationStage> installationViewModel = new();

                stageViewModels.AddRange(installationViewModel.Stages.Values);
                stageViewModels.InsertRange(1, vanillaStagesViewModel.Stages.Values);

                stagesViewModel = installationViewModel;
            }
            else throw new InvalidOperationException();

            return stageViewModels;
        }
}

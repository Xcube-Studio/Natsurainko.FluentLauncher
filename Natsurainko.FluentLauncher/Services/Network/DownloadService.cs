﻿using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Experimental.GameManagement.Downloader;
using Nrk.FluentCore.Experimental.GameManagement.Installer;
using Nrk.FluentCore.Experimental.GameManagement.Installer.Data;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.FabricInstanceInstaller;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.ForgeInstanceInstaller;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.OptiFineInstanceInstaller;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.QuiltInstanceInstaller;
using static Nrk.FluentCore.Experimental.GameManagement.Installer.VanillaInstanceInstaller;

namespace Natsurainko.FluentLauncher.Services.Network;

internal partial class DownloadService
{
    private readonly SettingsService _settingsService;
    private readonly GameService _gameService;
    private readonly INavigationService _navigationService;
    private readonly ObservableCollection<TaskViewModel> _downloadProcesses = [];
    private MultipartDownloader _downloader = new(HttpUtils.HttpClient, 1024 * 1024, 8, 64);

    public IDownloader Downloader { get => _downloader; }

    public ReadOnlyObservableCollection<TaskViewModel> DownloadProcesses { get; }

    public DownloadService(SettingsService settingsService, GameService gameService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _gameService = gameService;

        DownloadProcesses = new(_downloadProcesses);

        // TODO: 注册下载设置变化事件
    }

    public void DownloadResourceFile(GameResourceFile file, string filePath)
    {
        var taskViewModel = new DownloadGameResourceTaskViewModel(file, filePath);
        _downloadProcesses.Insert(0, taskViewModel);
        taskViewModel.Start();
    }

    public void InstallInstance(InstanceInstallConfig config)
    {
        var taskViewModel = new InstallInstanceTaskViewModel(
            GetInstanceInstaller(config, out var installationStageViews),
            config.InstanceId,
            installationStageViews);

        _downloadProcesses.Insert(0, taskViewModel);
        taskViewModel.Start();
    }

    IInstanceInstaller GetInstanceInstaller(
        InstanceInstallConfig instanceInstallConfig,
        out IReadOnlyList<InstallationStageViewModel> installationStageViews)
    {
        var versionManifestItem = instanceInstallConfig.ManifestItem;
        string minecraftFolder = _settingsService.ActiveMinecraftFolder ?? throw new InvalidOperationException();
        string javaPath = _settingsService.ActiveJava;

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

        installationStageViews = GetInstallationViewModel(modLoaderType, out var vanillaStagesViewModel, out var stagesViewModel);

        IInstanceInstaller installer = modLoaderType switch
        {
            ModLoaderType.Forge => new ForgeInstanceInstaller()
            {
                //DownloadMirror = DownloadMirrors.BmclApi,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (ForgeInstallData)selectedInstallData,
                Progress = (InstallationViewModel<ForgeInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                IsNeoForgeInstaller = false
            },
            ModLoaderType.NeoForge => new ForgeInstanceInstaller()
            {
                //DownloadMirror = DownloadMirrors.BmclApi,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (ForgeInstallData)selectedInstallData,
                Progress = (InstallationViewModel<ForgeInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath,
                IsNeoForgeInstaller = true
            },
            ModLoaderType.OptiFine => new OptiFineInstanceInstaller()
            {
                DownloadMirror = DownloadMirrors.BmclApi,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (OptiFineInstallData)selectedInstallData,
                Progress = (InstallationViewModel<OptiFineInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel,
                JavaPath = javaPath
            },
            ModLoaderType.Fabric => new FabricInstanceInstaller()
            {
                DownloadMirror = DownloadMirrors.BmclApi,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (FabricInstallData)selectedInstallData,
                Progress = (InstallationViewModel<FabricInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel
            },
            ModLoaderType.Quilt => new QuiltInstanceInstaller()
            {
                DownloadMirror = DownloadMirrors.BmclApi,
                McVersionManifestItem = versionManifestItem,
                MinecraftFolder = minecraftFolder,
                CheckAllDependencies = true,
                InstallData = (QuiltInstallData)selectedInstallData,
                Progress = (InstallationViewModel<QuiltInstallationStage>)stagesViewModel,
                VanillaInstallationProgress = vanillaStagesViewModel
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
            List<InstallationStageViewModel> stageViewModels = new();
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

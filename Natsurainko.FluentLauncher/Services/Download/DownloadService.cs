using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Install;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.DefaultComponents.Download;
using Nrk.FluentCore.DefaultComponents.Install;
using Nrk.FluentCore.Interfaces;
using Nrk.FluentCore.Services.Download;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace Natsurainko.FluentLauncher.Services.Download;

internal class DownloadService : DefaultDownloadService
{
    private new readonly SettingsService _settingsService;
    private readonly GameService _gameService;
    private readonly INavigationService _navigationService;
    private readonly ObservableCollection<DownloadProcess> _downloadProcesses = new();

    public ReadOnlyObservableCollection<DownloadProcess> DownloadProcesses { get; init; }

    public DownloadService(SettingsService settingsService, GameService gameService, INavigationService navigationService) : base(settingsService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _gameService = gameService;

        DownloadProcesses = new(_downloadProcesses);
    }

    public DefaultResourcesDownloader CreateResourcesDownloader(GameInfo gameInfo, IEnumerable<LibraryElement> libraryElements = null)
    {
        UpdateDownloadSettings();

        if (_settingsService.CurrentDownloadSource != "Mojang")
            return base.CreateResourcesDownloader(gameInfo, libraryElements, downloadMirrorSource:
                _settingsService.CurrentDownloadSource.Equals("Mcbbs") ? DownloadMirrors.Mcbbs : DownloadMirrors.Bmclapi);

        return base.CreateResourcesDownloader(gameInfo, libraryElements);
    }

    private void UpdateDownloadSettings()
    {
        HttpUtils.DownloadSetting.EnableLargeFileMultiPartDownload = _settingsService.EnableFragmentDownload;
        HttpUtils.DownloadSetting.MultiThreadsCount = _settingsService.MaxDownloadThreads;
    }

    public void DownloadResourceFile(object file, string filePath)
    {
        var process = new ResourceDownloadProcess(file, filePath);
        _downloadProcesses.Insert(0, process);
        _ = process.Start();
    }

    public void InstallCore(CoreInstallationInfo info)
    {
        string GetModLoaderPackageDownloadUrl(ChooseModLoaderData loaderData)
        {
            var downloadUrl = DownloadMirrors.Bmclapi.Domain;
            var metadata = loaderData.SelectedItem.Metadata;

            downloadUrl = loaderData.Type switch
            {
                ModLoaderType.Forge => $"{downloadUrl}/forge/download/{metadata.GetValue<int>()}",
                ModLoaderType.NeoForge => $"{downloadUrl}/neoforge/version/{metadata.GetValue<string>()}/download/installer.jar",
                ModLoaderType.OptiFine => $"{downloadUrl}/optifine/{metadata["mcversion"].GetValue<string>()}/{metadata["type"].GetValue<string>()}/{metadata["patch"].GetValue<string>()}",
                _ => throw new NotImplementedException()
            };

            return downloadUrl;
        }

        string GetTitle()
        {
            var title = info.ManifestItem.Id;

            if (info.PrimaryLoader != null)
                title += $", {info.PrimaryLoader.Type} {info.PrimaryLoader.SelectedItem.DisplayText}";

            if (info.SecondaryLoader != null)
                title += $", {info.SecondaryLoader.Type} {info.SecondaryLoader.SelectedItem.DisplayText}";

            return title;
        }

        var installProcess = new CoreInstallProcess() { Title = GetTitle() };
        var firstToStart = new List<CoreInstallProcess.ProgressItem>();

        GameInfo inheritsFrom = _gameService.GameInfos.FirstOrDefault(x => x.AbsoluteId.Equals(info.ManifestItem.Id));

        var installVanillaGame = new CoreInstallProcess.ProgressItem(@this =>
        {
            var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
            {
                AbsolutePath = Path.Combine(_gameService.ActiveMinecraftFolder, "versions", info.ManifestItem.Id, $"{info.ManifestItem.Id}.json"),
                Url = info.ManifestItem.Url
            },
            downloadSetting: new DownloadSetting
            {
                EnableLargeFileMultiPartDownload = false
            },
            perSecondProgressChangedAction: @this.OnProgressChanged);

            downloadTask.Wait();

            App.DispatcherQueue.SynchronousTryEnqueue(() => _gameService.RefreshCurrentFolder());
            inheritsFrom = _gameService.GameInfos.FirstOrDefault(x => x.AbsoluteId.Equals(info.ManifestItem.Id));

        }, ResourceUtils.GetValue("Converters", "_ProgressItem_InstallVanilla").Replace("${id}", info.ManifestItem.Id), installProcess);
        var completeResources = new CoreInstallProcess.ProgressItem(@this =>
        {
            int finished = 0;
            int total = 0;

            var resourcesDownloader = CreateResourcesDownloader(inheritsFrom);

            resourcesDownloader.SingleFileDownloaded += (_, _) => 
            {
                Interlocked.Increment(ref finished);
                @this.OnProgressChanged((double)finished / total);
            };
            resourcesDownloader.DownloadElementsPosted += (_, count) => App.DispatcherQueue.TryEnqueue(() =>
            {
                total = count;
                @this.OnProgressChanged(total != 0 ? (double)finished / total : 1);
            });

            resourcesDownloader.Download();

        }, ResourceUtils.GetValue("Converters", "_ProgressItem_CompleteResources"), installProcess);
        var setCoreConfig = new CoreInstallProcess.ProgressItem(@this =>
        {
            App.DispatcherQueue.SynchronousTryEnqueue(() => _gameService.RefreshCurrentFolder());

            var gameInfo = _gameService.GameInfos.First(x => x.AbsoluteId.Equals(info.AbsoluteId));
            var specialConfig = gameInfo.GetSpecialConfig();

            specialConfig.EnableSpecialSetting = info.EnableIndependencyCore || !string.IsNullOrEmpty(info.NickName);
            specialConfig.EnableIndependencyCore = info.EnableIndependencyCore;
            specialConfig.NickName = info.NickName;

            @this.OnProgressChanged(1);

        }, ResourceUtils.GetValue("Converters", "_ProgressItem_ApplySettings"), installProcess);

        firstToStart.Add(installVanillaGame);
        installProcess.Progresses.Add(installVanillaGame);
        installProcess.Progresses.Add(completeResources);

        if (info.PrimaryLoader != null)
        {
            var loaderFullName = $"{info.PrimaryLoader.Type}_{info.PrimaryLoader.SelectedItem.DisplayText}";

            var primaryLoaderFile = Path.Combine(
                LocalStorageService.LocalFolderPath, 
                "cache-downloads", 
                $"{loaderFullName}.jar");

            var runInstallExecutor = new CoreInstallProcess.ProgressItem(@this =>
            {
                IInstallExecutor executor = info.PrimaryLoader.Type switch
                {
                    ModLoaderType.Forge => new ForgeInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.NeoForge => new ForgeInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.OptiFine => new OptiFineInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.Fabric => new FabricInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        FabricBuild = info.PrimaryLoader.SelectedItem.Metadata.Deserialize<FabricInstallBuild>()
                    },
                    ModLoaderType.Quilt => new QuiltInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        QuiltBuild = info.PrimaryLoader.SelectedItem.Metadata.Deserialize<QuiltInstallBuild>()
                    },
                };

                executor.ProgressChanged += (_, e) => @this.OnProgressChanged(e);
                var task = executor.ExecuteAsync();
                task.Wait();
            }, ResourceUtils.GetValue("Converters", "_ProgressItem_RunExecutor").Replace("${type}", info.PrimaryLoader.Type.ToString()), installProcess);

            if (!(info.PrimaryLoader.Type == ModLoaderType.Fabric || info.PrimaryLoader.Type == ModLoaderType.Quilt))
            {
                var downloadInstallerPackage = new CoreInstallProcess.ProgressItem(@this =>
                {
                    var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
                    {
                        AbsolutePath = primaryLoaderFile,
                        Url = GetModLoaderPackageDownloadUrl(info.PrimaryLoader)
                    },
                    downloadSetting: new DownloadSetting
                    {
                        EnableLargeFileMultiPartDownload = false
                    },
                    perSecondProgressChangedAction: @this.OnProgressChanged);

                    downloadTask.Wait();

                }, ResourceUtils.GetValue("Converters", "_ProgressItem_DownloadPackage").Replace("${loader}", loaderFullName), installProcess);

                installProcess.Progresses.Add(downloadInstallerPackage);

                completeResources.SetNext(downloadInstallerPackage);
                downloadInstallerPackage.SetNext(runInstallExecutor);
            }
            else completeResources.SetNext(runInstallExecutor);

            runInstallExecutor.SetNext(setCoreConfig);
            installProcess.Progresses.Add(runInstallExecutor);
        }
        else completeResources.SetNext(setCoreConfig);

        installProcess.Progresses.Add(setCoreConfig);

        if (info.SecondaryLoader != null)
        {
            var loaderFullName = $"{info.SecondaryLoader.Type}_{info.SecondaryLoader.SelectedItem.DisplayText}";

            var secondaryLoaderFile = info.EnableIndependencyCore 
                ? Path.Combine(_gameService.ActiveMinecraftFolder, "versions", info.AbsoluteId, "mods", $"{loaderFullName}.jar")
                : Path.Combine(_gameService.ActiveMinecraftFolder, "mods", $"{loaderFullName}.jar");

            var downloadInstallerPackage = new CoreInstallProcess.ProgressItem(@this =>
            {
                var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
                {
                    AbsolutePath = secondaryLoaderFile,
                    Url = GetModLoaderPackageDownloadUrl(info.SecondaryLoader)
                },
                downloadSetting: new DownloadSetting
                {
                    EnableLargeFileMultiPartDownload = false
                },
                perSecondProgressChangedAction: @this.OnProgressChanged);

                downloadTask.Wait();
            }, ResourceUtils.GetValue("Converters", "_ProgressItem_DownloadPackage").Replace("${loader}", loaderFullName), installProcess);

            installProcess.Progresses.Add(downloadInstallerPackage);
            firstToStart.Add(downloadInstallerPackage);
        }

        installVanillaGame.SetNext(completeResources);

        foreach (var item in info.AdditionalOptions)
        {
            item.SetCoreInstallProcess(installProcess);
            installProcess.Progresses.Add(item);
            firstToStart.Add(item);
        }

        installProcess.SetStartAction(@this =>
        {
            foreach (var progress in firstToStart)
                progress.Start();
        });

        _downloadProcesses.Insert(0, installProcess);
        installProcess.Start();
    }
}

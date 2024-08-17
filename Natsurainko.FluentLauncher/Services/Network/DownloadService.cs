using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Management.Downloader;
using Nrk.FluentCore.Management.Downloader.Data;
using Nrk.FluentCore.Management.ModLoaders;
using Nrk.FluentCore.Management.Parsing;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;

namespace Natsurainko.FluentLauncher.Services.Network;

internal partial class DownloadService
{
    private readonly SettingsService _settingsService;
    private readonly GameService _gameService;
    private readonly INavigationService _navigationService;
    private readonly ObservableCollection<DownloadProcessViewModel> _downloadProcesses = new();

    public ReadOnlyObservableCollection<DownloadProcessViewModel> DownloadProcesses { get; init; }

    public DownloadService(SettingsService settingsService, GameService gameService, INavigationService navigationService)
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
            return Base_CreateResourcesDownloader(gameInfo, libraryElements, downloadMirrorSource: DownloadMirrors.Bmclapi);

        return Base_CreateResourcesDownloader(gameInfo, libraryElements);
    }

    private void UpdateDownloadSettings()
    {
        HttpUtils.DownloadSetting.EnableLargeFileMultiPartDownload = _settingsService.EnableFragmentDownload;
        HttpUtils.DownloadSetting.MultiThreadsCount = _settingsService.MaxDownloadThreads;
    }

    public void DownloadResourceFile(ResourceFileItem file, string filePath)
    {
        var process = new FileDownloadProcessViewModel(file, filePath);
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

        var installProcess = new InstallProcessViewModel() { Title = GetTitle() };
        var firstToStart = new List<InstallProcessViewModel.ProgressItem>();

        GameInfo inheritsFrom = _gameService.Games.FirstOrDefault(x => x.AbsoluteId.Equals(info.ManifestItem.Id));

        var installVanillaGame = new InstallProcessViewModel.ProgressItem(@this =>
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

            App.DispatcherQueue.SynchronousTryEnqueue(() => _gameService.RefreshGames());
            inheritsFrom = _gameService.Games.FirstOrDefault(x => x.AbsoluteId.Equals(info.ManifestItem.Id));

        }, ResourceUtils.GetValue("Converters", "_ProgressItem_InstallVanilla").Replace("${id}", info.ManifestItem.Id), installProcess);
        var completeResources = new InstallProcessViewModel.ProgressItem(@this =>
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
        var setCoreConfig = new InstallProcessViewModel.ProgressItem(@this =>
        {
            App.DispatcherQueue.SynchronousTryEnqueue(() => _gameService.RefreshGames());

            var gameInfo = _gameService.Games.First(x => x.AbsoluteId.Equals(info.AbsoluteId));
            var config = gameInfo.GetConfig();

            config.EnableSpecialSetting = info.EnableIndependencyCore || !string.IsNullOrEmpty(info.NickName);
            config.EnableIndependencyCore = info.EnableIndependencyCore;
            config.NickName = info.NickName;

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

            var runInstallExecutor = new InstallProcessViewModel.ProgressItem(@this =>
            {
                IModLoaderInstaller executor = info.PrimaryLoader.Type switch
                {
                    ModLoaderType.Forge => new ForgeInstaller
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.NeoForge => new ForgeInstaller
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.OptiFine => new OptiFineInstaller
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.Fabric => new FabricInstaller
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        FabricBuild = info.PrimaryLoader.SelectedItem.Metadata.Deserialize<FabricInstallBuild>()
                    },
                    ModLoaderType.Quilt => new QuiltInstaller
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        QuiltBuild = info.PrimaryLoader.SelectedItem.Metadata.Deserialize<QuiltInstallBuild>()
                    },
                    _ => throw new NotImplementedException()
                };

                executor.ProgressChanged += (_, e) => @this.OnProgressChanged(e);
                var task = executor.ExecuteAsync();
                task.Wait();
            }, ResourceUtils.GetValue("Converters", "_ProgressItem_RunExecutor").Replace("${type}", info.PrimaryLoader.Type.ToString()), installProcess);

            if (!(info.PrimaryLoader.Type == ModLoaderType.Fabric || info.PrimaryLoader.Type == ModLoaderType.Quilt))
            {
                var downloadInstallerPackage = new InstallProcessViewModel.ProgressItem(@this =>
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

            var downloadInstallerPackage = new InstallProcessViewModel.ProgressItem(@this =>
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

internal partial class DownloadService
{
    private DefaultResourcesDownloader Base_CreateResourcesDownloader(GameInfo gameInfo,
    IEnumerable<LibraryElement>? libraryElements = default,
    IEnumerable<AssetElement>? assetElements = default,
    DownloadMirrorSource? downloadMirrorSource = default)
    {
        List<LibraryElement> libraries = libraryElements?.ToList() ?? [];

        if (libraryElements == null)
        {
            var libraryParser = new DefaultLibraryParser(gameInfo);
            libraryParser.EnumerateLibraries(out var enabledLibraries, out var enabledNativesLibraries);

            libraries = enabledLibraries.Union(enabledNativesLibraries).ToList();
        }

        if (assetElements == null)
        {
            var assetParser = new DefaultAssetParser(gameInfo);
            var assetElement = assetParser.GetAssetIndexJson();
            if (downloadMirrorSource != null)
                assetElement.Url?.ReplaceFromDictionary(downloadMirrorSource.AssetsReplaceUrl);

            if (!assetElement.VerifyFile())
            {
                var assetIndexDownloadTask = HttpUtils.DownloadElementAsync(assetElement);
                assetIndexDownloadTask.Wait();

                if (assetIndexDownloadTask.Result.IsFaulted)
                    throw new System.Exception("依赖材质索引文件获取失败");
            }

            assetElements = assetParser.EnumerateAssets();
        }

        var jar = gameInfo.GetJarElement();
        if (jar != null && !jar.VerifyFile())
            libraries.Add(jar);

        var defaultResourcesDownloader = new DefaultResourcesDownloader(gameInfo);

        defaultResourcesDownloader.SetLibraryElements(libraries);
        defaultResourcesDownloader.SetAssetsElements(assetElements);

        if (downloadMirrorSource != null) defaultResourcesDownloader.SetDownloadMirror(downloadMirrorSource);

        return defaultResourcesDownloader;
    }}

internal static class IDownloadElementExtensions
{
    /// <summary>
    /// 验证文件
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static bool VerifyFile(this IDownloadElement element)
    {
        if (!File.Exists(element.AbsolutePath))
            return false;

        if (!string.IsNullOrEmpty(element.Checksum))
        {
            using var fileStream = File.OpenRead(element.AbsolutePath);

            return BitConverter.ToString(SHA1.HashData(fileStream)).Replace("-", string.Empty)
                .ToLower().Equals(element.Checksum);
        }

        return true;
    }
}
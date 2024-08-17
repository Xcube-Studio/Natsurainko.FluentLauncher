using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Experimental.GameManagement.Dependencies;
using Nrk.FluentCore.Experimental.GameManagement.Downloader;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders.Fabric;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders.Forge;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders.OptiFine;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders.Quilt;
using Nrk.FluentCore.Management;
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

    private MultipartDownloader _downloader = new MultipartDownloader(HttpUtils.HttpClient, 1024 * 1024, 8, 64);
    public IDownloader Downloader { get => _downloader; }

    public ReadOnlyObservableCollection<DownloadProcessViewModel> DownloadProcesses { get; init; }

    public DownloadService(SettingsService settingsService, GameService gameService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _gameService = gameService;

        DownloadProcesses = new(_downloadProcesses);
    }

    public DependencyResolver CreateResourcesDownloader(MinecraftInstance instance, IEnumerable<MinecraftLibrary>? libraryElements = null)
    {
        UpdateDownloadSettings();

        // TODO: Move this part to DependencyResolver
        List<MinecraftDependency> dependencies = new();
        if (libraryElements is not null)
            dependencies.AddRange(libraryElements);

        if (libraryElements == null)
        {
            var (libs, nativeLibs) = instance.GetRequiredLibraries();
            dependencies.AddRange(libs.Union(nativeLibs));
        }

        var assetElement = instance.GetAssetIndex();

        if (!DependencyResolver.VerifyDependencyAsync(assetElement).GetAwaiter().GetResult())
        {
            var result = _downloader.CreateDownloadTask(assetElement.Url, assetElement.FullPath).StartAsync().GetAwaiter().GetResult();

            if (result.Type == DownloadResultType.Failed)
                throw new System.Exception("依赖材质索引文件获取失败");
        }

        var assetElements = instance.GetRequiredAssets();
        dependencies.AddRange(assetElements);

        var jar = instance.GetJarElement();
        if (jar != null && !DependencyResolver.VerifyDependencyAsync(jar).GetAwaiter().GetResult())
            dependencies.Add(jar);

        DependencyResolver depResolver = new(dependencies);
        return depResolver;
    }

    private void UpdateDownloadSettings()
    {
        _downloader = new MultipartDownloader(
            HttpUtils.HttpClient,
            1024 * 1024,
            _settingsService.EnableFragmentDownload ? _settingsService.MaxDownloadThreads : 1,
            64);
    }

    public void DownloadResourceFile(object file, string filePath)
    {
        var process = new FileDownloadProcessViewModel(file, filePath);
        _downloadProcesses.Insert(0, process);
        _ = process.Start();
    }

    public void InstallCore(CoreInstallationInfo info)
    {
        string GetModLoaderPackageDownloadUrl(ChooseModLoaderData loaderData)
        {
            var downloadUrl = "https://bmclapi2.bangbang93.com/";
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

        MinecraftInstance inheritsFrom = _gameService.Games.FirstOrDefault(x => x.VersionFolderName.Equals(info.ManifestItem.Id));

        var installVanillaGame = new InstallProcessViewModel.ProgressItem(@this =>
        {
            var downloadTask = _downloader.CreateDownloadTask(
                info.ManifestItem.Url,
                Path.Combine(_gameService.ActiveMinecraftFolder, "versions", info.ManifestItem.Id, $"{info.ManifestItem.Id}.json"));

            Timer t = new((_) =>
            {
                if (downloadTask.TotalBytes is null)
                    return;
                @this.OnProgressChanged(downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            downloadTask.StartAsync().Wait();
            t.Dispose();

            App.DispatcherQueue.SynchronousTryEnqueue(() => _gameService.RefreshGames());
            inheritsFrom = _gameService.Games.FirstOrDefault(x => x.VersionFolderName.Equals(info.ManifestItem.Id));

        }, ResourceUtils.GetValue("Converters", "_ProgressItem_InstallVanilla").Replace("${id}", info.ManifestItem.Id), installProcess);
        var completeResources = new InstallProcessViewModel.ProgressItem(@this =>
        {
            int finished = 0;
            int total = 0;

            var resourcesDownloader = CreateResourcesDownloader(inheritsFrom);

            resourcesDownloader.DependencyDownloaded += (_, _) =>
            {
                Interlocked.Increment(ref finished);
                @this.OnProgressChanged((double)finished / total);
            };
            resourcesDownloader.InvalidDependenciesDetermined += (_, deps) => App.DispatcherQueue.TryEnqueue(() =>
            {
                total = deps.Count();
                @this.OnProgressChanged(total != 0 ? (double)finished / total : 1);
            });

            resourcesDownloader.VerifyAndDownloadDependenciesAsync(_downloader, 10).GetAwaiter().GetResult();

        }, ResourceUtils.GetValue("Converters", "_ProgressItem_CompleteResources"), installProcess);
        var setCoreConfig = new InstallProcessViewModel.ProgressItem(@this =>
        {
            App.DispatcherQueue.SynchronousTryEnqueue(() => _gameService.RefreshGames());

            var instance = _gameService.Games.First(x => x.VersionFolderName.Equals(info.AbsoluteId));
            var config = instance.GetConfig();

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
                    ModLoaderType.Forge => new ForgeInstaller(_downloader)
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedInstance = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.NeoForge => new ForgeInstaller(_downloader)
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedInstance = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.OptiFine => new OptiFineInstaller
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedInstance = inheritsFrom,
                        JavaPath = _settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.Fabric => new FabricInstaller(_downloader)
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedInstance = inheritsFrom,
                        FabricBuild = info.PrimaryLoader.SelectedItem.Metadata.Deserialize<FabricInstallBuild>()
                    },
                    ModLoaderType.Quilt => new QuiltInstaller(_downloader)
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedInstance = inheritsFrom,
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
                    var downloadTask = _downloader.CreateDownloadTask(GetModLoaderPackageDownloadUrl(info.PrimaryLoader), primaryLoaderFile);
                    Timer t = new((_) =>
                    {
                        if (downloadTask.TotalBytes is null)
                            return;
                        @this.OnProgressChanged(downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                    downloadTask.StartAsync().Wait();
                    t.Dispose();
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
                var downloadTask = _downloader.CreateDownloadTask(GetModLoaderPackageDownloadUrl(info.SecondaryLoader), secondaryLoaderFile);
                Timer t = new((_) =>
                {
                    if (downloadTask.TotalBytes is null)
                        return;
                    @this.OnProgressChanged(downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                downloadTask.StartAsync().Wait();
                t.Dispose();
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

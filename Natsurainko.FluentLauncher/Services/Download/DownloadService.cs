using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace Natsurainko.FluentLauncher.Services.Download;

internal class DownloadService : DefaultDownloadService
{
    private new readonly SettingsService _settingsService;
    private readonly ObservableCollection<DownloadProcess> _downloadProcesses = new();

    public ReadOnlyObservableCollection<DownloadProcess> DownloadProcesses { get; init; }

    public DownloadService(SettingsService settingsService) : base(settingsService)
    {
        _settingsService = settingsService;

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

    public void CreateDownloadProcessFromResourceFile(object file, string filePath)
    {
        var process = new ResourceDownloadProcess(file, filePath);
        _downloadProcesses.Insert(0, process);
        _ = process.Start();
    }

    public void CreateCoreInstallProcess(CoreInstallationInfo info)
    {
        string GetFabricInstallerPackage()
        {
            var httpResponseMessage = HttpUtils.HttpGet("https://meta.fabricmc.net/v2/versions/installer");
            return JsonNode.Parse(httpResponseMessage.Content.ReadAsString()).AsArray()[0]["url"].GetValue<string>();
        }

        string GetModLoaderPackageDownloadUrl(ChooseModLoaderData loaderData)
        {
            var downloadUrl = DownloadMirrors.Bmclapi.Domain;
            var metadata = loaderData.SelectedItem.Metadata;

            downloadUrl += loaderData.Type switch
            {
                ModLoaderType.Forge => $"/forge/download/{metadata.GetValue<int>().ToString()}",
                ModLoaderType.NeoForge => $"/neoforge/version/{loaderData._manifestItem.Id}/download/{metadata.GetValue<string>()}",
                ModLoaderType.OptiFine => $"/optifine/{metadata["mcversion"].GetValue<string>()}/{metadata["type"].GetValue<string>()}/{metadata["patch"].GetValue<string>()}",
                ModLoaderType.Fabric => GetFabricInstallerPackage(),
                _ => throw new NotImplementedException()
            };

            return downloadUrl;
        }

        string GetTitle()
        {
            var title = $"Install {info.ManifestItem.Id}";

            if (info.PrimaryLoader != null)
                title += $", {info.PrimaryLoader.Type} {info.PrimaryLoader.SelectedItem.DisplayText}";

            if (info.SecondaryLoader != null)
                title += $", {info.SecondaryLoader.Type} {info.SecondaryLoader.SelectedItem.DisplayText}";

            return title;
        }

        var gameService = App.GetService<GameService>();
        var settingsService = App.GetService<SettingsService>();
            
        var installProcess = new CoreInstallProcess() { Title = GetTitle(), DisplayState = "Has not started" };
        var firstToStart = new List<CoreInstallProcess.ProgressItem>();

        GameInfo inheritsFrom = gameService.GameInfos.FirstOrDefault(x => x.AbsoluteId.Equals(info.ManifestItem.Id));

        var installVanillaGame = new CoreInstallProcess.ProgressItem(@this =>
        {
            var downloadTask = HttpUtils.DownloadElementAsync(new DownloadElement
            {
                AbsolutePath = Path.Combine(gameService.ActiveMinecraftFolder, "versions", info.ManifestItem.Id, $"{info.ManifestItem.Id}.json"),
                Url = info.ManifestItem.Url
            },
            downloadSetting: new DownloadSetting
            {
                EnableLargeFileMultiPartDownload = false
            },
            perSecondProgressChangedAction: @this.OnProgressChanged);

            downloadTask.Wait();

            App.DispatcherQueue.SynchronousTryEnqueue(() => gameService.RefreshCurrentFolder());
            inheritsFrom = gameService.GameInfos.FirstOrDefault(x => x.AbsoluteId.Equals(info.ManifestItem.Id));

        }, $"Install Vanilla Game {info.ManifestItem.Id}", installProcess);
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
                @this.OnProgressChanged((double)finished / total);
            });

            resourcesDownloader.Download();

        }, "Complete Resources", installProcess);

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
                        JavaPath = settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.NeoForge => new ForgeInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.OptiFine => new OptiFineInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
                    },
                    ModLoaderType.Fabric => new FabricInstallExecutor
                    {
                        AbsoluteId = info.AbsoluteId,
                        InheritedFrom = inheritsFrom,
                        JavaPath = settingsService.ActiveJava,
                        PackageFilePath = primaryLoaderFile
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
            }, "Run Install Executor", installProcess);

            if (info.PrimaryLoader.Type != ModLoaderType.Quilt)
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

                }, $"Download {loaderFullName} Installer Package", installProcess);

                installProcess.Progresses.Add(downloadInstallerPackage);

                completeResources.SetNext(downloadInstallerPackage);
                downloadInstallerPackage.SetNext(runInstallExecutor);
            }
            else completeResources.SetNext(runInstallExecutor);

            installProcess.Progresses.Add(runInstallExecutor);
        }

        if (info.SecondaryLoader != null)
        {
            var loaderFullName = $"{info.SecondaryLoader.Type}_{info.SecondaryLoader.SelectedItem.DisplayText}";

            var secondaryLoaderFile = info.EnableIndependencyCore 
                ? Path.Combine(gameService.ActiveMinecraftFolder, "versions", info.AbsoluteId, "mods", $"{loaderFullName}.jar")
                : Path.Combine(gameService.ActiveMinecraftFolder, "mods", $"{loaderFullName}.jar");

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
            }, $"Download {loaderFullName} Installer Package", installProcess);

            installProcess.Progresses.Add(downloadInstallerPackage);
            firstToStart.Add(downloadInstallerPackage);
        }

        installVanillaGame.SetNext(completeResources);

        installProcess.SetStartAction(@this =>
        {
            foreach (var progress in firstToStart)
                progress.Start();
        });

        _downloadProcesses.Insert(0, installProcess);
        installProcess.Start();

        Views.ShellPage.ContentFrame.Navigate(typeof(Views.Activities.ActivitiesNavigationPage), typeof(Views.Activities.DownloadPage));
    }
}

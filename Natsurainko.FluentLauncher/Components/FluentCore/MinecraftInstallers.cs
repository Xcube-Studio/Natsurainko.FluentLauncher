using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Install;
using Natsurainko.FluentCore.Model.Install.Fabric;
using Natsurainko.FluentCore.Model.Install.Forge;
using Natsurainko.FluentCore.Model.Install.OptiFine;
using Natsurainko.FluentCore.Model.Install.Quilt;
using Natsurainko.FluentCore.Model.Install.Vanilla;
using Natsurainko.FluentCore.Model.Parser;
using Natsurainko.FluentLauncher.Components.CrossProcess;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Downloader;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

public class MinecraftVanlliaInstaller : Natsurainko.FluentCore.Module.Installer.MinecraftVanlliaInstaller
{
    public MinecraftVanlliaInstaller(
        IGameCoreLocator<IGameCore> coreLocator,
        string mcVersion,
        string customId = default)
        : base(coreLocator, mcVersion, customId) { }

    public MinecraftVanlliaInstaller(
        IGameCoreLocator<IGameCore> coreLocator,
        CoreManifestItem coreManifestItem,
        string customId = default)
        : base(coreLocator, coreManifestItem, customId) { }

    public override Task<GameCoreInstallerResponse> InstallAsync()
    {
        VersionJsonEntity jsonEntity = default;
        FileInfo jsonFile = default;
        IGameCore gameCore = default;
        ParallelDownloaderResponse parallelDownloaderResponse = default;
        Stopwatch stopwatch = Stopwatch.StartNew();

        async Task GetCoreJson()
        {
            OnProgressChanged("Get Core Json", 0);

            using var responseMessage = await HttpWrapper.HttpGetAsync(CoreManifestItem.Url);
            responseMessage.EnsureSuccessStatusCode();

            jsonEntity = JsonConvert.DeserializeObject<VersionJsonEntity>(await responseMessage.Content.ReadAsStringAsync());

            if (!string.IsNullOrEmpty(CustomId))
                jsonEntity.Id = CustomId;

            jsonFile = new(Path.Combine(GameCoreLocator.Root.FullName, "versions", jsonEntity.Id, $"{jsonEntity.Id}.json")); ;

            if (!jsonFile.Directory.Exists)
                jsonFile.Directory.Create();

            File.WriteAllText(jsonFile.FullName, jsonEntity.ToJson());
            gameCore = GameCoreLocator.GetGameCore(jsonEntity.Id);

            OnProgressChanged("Get Core Json", 1);
        }

        async Task DownloadResources()
        {
            OnProgressChanged("Download Resources", 0);

            var resourceDownloader = new CrossProcessResourceDownloader(gameCore, App.GetService<SettingsService>());

            resourceDownloader.DownloadProgressChanged += (sender, e)
                => OnProgressChanged($"Download Resources", e.Progress, e.TotleTasks, e.CompletedTasks);

            parallelDownloaderResponse = await resourceDownloader.DownloadAsync();

            OnProgressChanged("Download Resources", 1);
        }

        return Task.Run<GameCoreInstallerResponse>(async () =>
        {
            await GetCoreJson();
            await DownloadResources();

            stopwatch.Stop();

            return new()
            {
                Success = true,
                GameCore = gameCore,
                Stopwatch = stopwatch,
                DownloaderResponse = parallelDownloaderResponse
            };
        }).ContinueWith(task =>
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();

            return task.IsFaulted ? new()
            {
                Success = false,
                Stopwatch = stopwatch,
                Exception = task.Exception
            } : task.Result;
        });
    }
}

public class MinecraftForgeInstaller : Natsurainko.FluentCore.Module.Installer.MinecraftForgeInstaller
{
    public MinecraftForgeInstaller(
        IGameCoreLocator<IGameCore> coreLocator,
        ForgeInstallBuild build,
        string javaPath,
        string packageFile = null,
        string customId = null) : base(coreLocator, build, javaPath, packageFile, customId) { }

    protected override async Task CheckInheritedCore()
    {
        OnProgressChanged("Check Inherited Core", 0);

        if (GameCoreLocator.GetGameCore(McVersion) == null)
        {
            var installer = new MinecraftVanlliaInstaller(GameCoreLocator, McVersion);
            installer.ProgressChanged += (sender, e)
                => OnProgressChanged(
                    $"Check Inherited Core",
                    e.TotleProgress,
                    e.StepsProgress.Values.Sum(x => x.TotleTask),
                    e.StepsProgress.Values.Sum(x => x.CompletedTask));


            var installerResponse = await installer.InstallAsync();
        }

        OnProgressChanged("Check Inherited Core", 1);
    }
}

public class MinecraftOptiFineInstaller : Natsurainko.FluentCore.Module.Installer.MinecraftOptiFineInstaller
{
    public MinecraftOptiFineInstaller(
        IGameCoreLocator<IGameCore> coreLocator,
        OptiFineInstallBuild build,
        string javaPath,
        string packageFile = null,
        string customId = null) : base(coreLocator, build, javaPath, packageFile, customId) { }

    protected override async Task CheckInheritedCore()
    {
        OnProgressChanged("Check Inherited Core", 0);

        if (GameCoreLocator.GetGameCore(McVersion) == null)
        {
            var installer = new MinecraftVanlliaInstaller(GameCoreLocator, McVersion);
            installer.ProgressChanged += (sender, e)
                => OnProgressChanged(
                    $"Check Inherited Core",
                    e.TotleProgress,
                    e.StepsProgress.Values.Sum(x => x.TotleTask),
                    e.StepsProgress.Values.Sum(x => x.CompletedTask));


            var installerResponse = await installer.InstallAsync();
        }

        OnProgressChanged("Check Inherited Core", 1);
    }
}

public class MinecraftFabricInstaller : Natsurainko.FluentCore.Module.Installer.MinecraftFabricInstaller
{
    public MinecraftFabricInstaller(
        IGameCoreLocator<IGameCore> coreLocator,
        FabricInstallBuild fabricInstallBuild,
        string customId = null) : base(coreLocator, fabricInstallBuild, customId) { }

    protected override async Task CheckInheritedCore()
    {
        OnProgressChanged("Check Inherited Core", 0);

        if (GameCoreLocator.GetGameCore(McVersion) == null)
        {
            var installer = new MinecraftVanlliaInstaller(GameCoreLocator, McVersion);
            installer.ProgressChanged += (sender, e)
                => OnProgressChanged(
                    $"Check Inherited Core",
                    e.TotleProgress,
                    e.StepsProgress.Values.Sum(x => x.TotleTask),
                    e.StepsProgress.Values.Sum(x => x.CompletedTask));


            var installerResponse = await installer.InstallAsync();
        }

        OnProgressChanged("Check Inherited Core", 1);
    }
}

public class MinecraftQuiltInstaller : Natsurainko.FluentCore.Module.Installer.MinecraftQuiltInstaller
{
    public MinecraftQuiltInstaller(
        IGameCoreLocator<IGameCore> coreLocator,
        QuiltInstallBuild quiltInstallBuild,
        string customId = null) : base(coreLocator, quiltInstallBuild, customId) { }

    protected override async Task CheckInheritedCore()
    {
        OnProgressChanged("Check Inherited Core", 0);

        if (GameCoreLocator.GetGameCore(McVersion) == null)
        {
            var installer = new MinecraftVanlliaInstaller(GameCoreLocator, McVersion);
            installer.ProgressChanged += (sender, e)
                => OnProgressChanged(
                    $"Check Inherited Core",
                    e.TotleProgress,
                    e.StepsProgress.Values.Sum(x => x.TotleTask),
                    e.StepsProgress.Values.Sum(x => x.CompletedTask));


            var installerResponse = await installer.InstallAsync();
        }

        OnProgressChanged("Check Inherited Core", 1);
    }
}


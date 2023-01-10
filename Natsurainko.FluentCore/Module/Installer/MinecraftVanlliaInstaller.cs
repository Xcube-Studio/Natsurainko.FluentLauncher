using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Install.Vanilla;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Downloader;
using Natsurainko.FluentCore.Service;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Installer
{
    public class MinecraftVanlliaInstaller : InstallerBase
    {
        public CoreManifestItem CoreManifestItem { get; private set; }

        public MinecraftVanlliaInstaller(IGameCoreLocator coreLocator, string id) : base(coreLocator)
        {
            GetCoreManifest().GetAwaiter().GetResult().Cores.ToList().ForEach(x =>
            {
                if (x.Id == id)
                    CoreManifestItem = x;
            });
        }

        public MinecraftVanlliaInstaller(IGameCoreLocator coreLocator, CoreManifestItem coreManifestItem) : base(coreLocator)
        {
            CoreManifestItem = coreManifestItem;
        }

        public override async Task<InstallerResponse> InstallAsync()
        {
            try
            {
                #region Write Core JSON

                OnProgressChanged("Getting Core Json", 0.1f);

                using var responseMessage = await HttpWrapper.HttpGetAsync(CoreManifestItem.Url);
                responseMessage.EnsureSuccessStatusCode();

                var entity = JsonConvert.DeserializeObject<VersionJsonEntity>(await responseMessage.Content.ReadAsStringAsync());

                if (!string.IsNullOrEmpty(this.CustomId))
                    entity.Id = this.CustomId;

                OnProgressChanged("Writing Core Json", 0.15f);

                var versionJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, $"{entity.Id}.json"));

                if (!versionJsonFile.Directory.Exists)
                    versionJsonFile.Directory.Create();

                File.WriteAllText(versionJsonFile.FullName, entity.ToJson());

                #endregion

                #region Download Resources

                OnProgressChanged("Downloading Resources", 0.2f);

                var resourceDownloader = new ResourceDownloader(GameCoreLocator.GetGameCore(entity.Id))
                {
                    DownloadProgressChangedAction = (message, progress) => OnProgressChanged($"Downloading Resources {message}", 0.2f + progress * 0.8f),
                };

                await resourceDownloader.DownloadAsync();

                OnProgressChanged("Finished", 1.0f);

                #endregion

                return new InstallerResponse
                {
                    Success = true,
                    GameCore = GameCoreLocator.GetGameCore(entity.Id),
                    Exception = null
                };
            }
            catch (Exception ex)
            {
                return new InstallerResponse
                {
                    Success = false,
                    GameCore = null,
                    Exception = ex
                };
            }
        }

        public static async Task<CoreManifest> GetCoreManifest()
        {
            using var res = await HttpWrapper.HttpGetAsync(DownloadApiManager.Current.VersionManifest);
            var entity = JsonConvert.DeserializeObject<CoreManifest>(await res.Content.ReadAsStringAsync());

            foreach (var core in entity.Cores)
                if (DownloadApiManager.Current.Host != DownloadApiManager.Mojang.Host)
                    core.Url = core.Url
                        .Replace("https://piston-meta.mojang.com", DownloadApiManager.Current.Host)
                        .Replace("https://launchermeta.mojang.com", DownloadApiManager.Current.Host);

            return entity;
        }
    }
}

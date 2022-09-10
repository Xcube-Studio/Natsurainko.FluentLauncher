using Natsurainko.FluentCore.Class.Model.Install.Vanilla;
using Natsurainko.FluentCore.Class.Model.Mod;
using Natsurainko.FluentCore.Module.Installer;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Class.AppData;

public static class CacheResources
{
    public static List<NewsViewData> NewsViewDatas { get; private set; }

    public static List<CurseForgeModpackViewData> CurseForgeModpackViewDatas { get; private set; }

    public static CoreManifest CoreManifest { get; set; }

    public static string[] FabricSupportedMcVersions { get; set; }

    public static string[] ForgeSupportedMcVersions { get; set; }

    public static string[] OptiFineSupportedMcVersions { get; set; }

    public static List<LauncherProcessViewData> LauncherProcesses { get; private set; } = new List<LauncherProcessViewData>();

    public static List<DownloaderProcessViewData> DownloaderProcesses { get; private set; } = new List<DownloaderProcessViewData>();

    public static async Task BeginDownloadNews()
    {
        using var res = await HttpWrapper.HttpGetAsync("https://launchercontent.mojang.com/news.json");

        NewsViewDatas = ((JArray)JObject.Parse(await res.Content.ReadAsStringAsync())["entries"])
            .Select(x => JsonConvert.DeserializeObject<NewsModel>(x.ToString()))
            .Take(20)
            .CreateCollectionViewData<NewsModel, NewsViewData>()
            .ToList();
    }

    public static async Task BeginDownloadCurseForgeModpackViewDatas()
    {
        CurseForgeModpackViewDatas = (await GlobalResources.CurseForgeModpackFinder.GetFeaturedModpacksAsync())
            .CreateCollectionViewData<CurseForgeModpack, CurseForgeModpackViewData>()
            .ToList();
    }

    public static async Task BeginDownloadCoreManifest()
    {
        CoreManifest ??= await MinecraftVanlliaInstaller.GetCoreManifest();

        await Task.Run(async () =>
        {
            var tasks = new Task<string[]>[]
            {
                MinecraftForgeInstaller.GetSupportedMcVersionsAsync(),
                MinecraftOptiFineInstaller.GetSupportedMcVersionsAsync(),
                MinecraftFabricInstaller.GetSupportedMcVersionsAsync()
            };

            ForgeSupportedMcVersions = await tasks[0];
            OptiFineSupportedMcVersions = await tasks[1];
            FabricSupportedMcVersions = await tasks[2];
        });
    }
}

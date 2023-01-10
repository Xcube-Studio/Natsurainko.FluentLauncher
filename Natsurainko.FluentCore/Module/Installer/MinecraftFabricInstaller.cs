using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Install.Fabric;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Interface;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Installer;

public class MinecraftFabricInstaller : InstallerBase
{
    public FabricInstallBuild FabricBuild { get; private set; }

    public MinecraftFabricInstaller(IGameCoreLocator coreLocator, FabricInstallBuild fabricInstallBuild, string customId = null) : base(coreLocator, customId)
    {
        FabricBuild = fabricInstallBuild;
    }

    public override async Task<InstallerResponse> InstallAsync()
    {
        #region Parse Build

        OnProgressChanged("Parse Build", 0.25f);

        var libraries = FabricBuild.LauncherMeta.Libraries["common"];

        if (FabricBuild.LauncherMeta.Libraries["common"] != null)
            libraries.AddRange(FabricBuild.LauncherMeta.Libraries["client"]);

        libraries.Insert(0, new() { Name = FabricBuild.Intermediary.Maven });
        libraries.Insert(0, new() { Name = FabricBuild.Loader.Maven });

        string mainClass = FabricBuild.LauncherMeta.MainClass.Type == JTokenType.Object
            ? FabricBuild.LauncherMeta.MainClass.ToObject<Dictionary<string, string>>()["client"]
            : string.IsNullOrEmpty(FabricBuild.LauncherMeta.MainClass.ToString())
                ? "net.minecraft.client.main.Main"
                : FabricBuild.LauncherMeta.MainClass.ToString();

        string inheritsFrom = FabricBuild.Intermediary.Version;

        if (mainClass == "net.minecraft.client.main.Main")
            return new()
            {
                Success = false,
                GameCore = null,
                Exception = new ArgumentNullException("MainClass")
            };

        #endregion

        #region Download Libraries

        OnProgressChanged("Downloading Libraries", 0.45f);

        libraries.ForEach(x => x.Url = UrlExtension.Combine("https://maven.fabricmc.net", UrlExtension.Combine(LibraryResource.FormatName(x.Name).ToArray())));

        var downloader = new MultithreadedDownloader<LibraryResource>
            (x => x.ToDownloadRequest(), libraries.Select(y => new LibraryResource { Root = GameCoreLocator.Root, Name = y.Name, Url = y.Url }).ToList());
        downloader.ProgressChanged += (object sender, (float, string) e) => OnProgressChanged($"Downloading Libraries {e.Item2}", 0.45f + 0.25f * e.Item1);

        var multithreadedDownload = await downloader.DownloadAsync();

        #endregion

        #region Check Inherited Core

        await CheckInheritedCore(0.7f, 0.85f, FabricBuild.Intermediary.Version);

        #endregion

        #region Write Files

        OnProgressChanged("Writing Files", 0.85f);

        var entity = new VersionJsonEntity
        {
            Id = string.IsNullOrEmpty(CustomId) ? $"fabric-loader-{FabricBuild.Loader.Version}-{FabricBuild.Intermediary.Version}" : CustomId,
            InheritsFrom = inheritsFrom,
            ReleaseTime = DateTime.Now.ToString("O"),
            Time = DateTime.Now.ToString("O"),
            Type = "release",
            JavaVersion = null,
            MainClass = mainClass,
            Arguments = new() { Jvm = new() { "-DFabricMcEmu= net.minecraft.client.main.Main" } },
            Libraries = libraries
        };

        var versionJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, $"{entity.Id}.json"));

        if (!versionJsonFile.Directory.Exists)
            versionJsonFile.Directory.Create();

        File.WriteAllText(versionJsonFile.FullName, entity.ToJson());

        #endregion

        OnProgressChanged("Finished", 1.0f);

        return new()
        {
            Success = true,
            GameCore = GameCoreLocator.GetGameCore(entity.Id),
            Exception = null
        };
    }

    public static async Task<string[]> GetSupportedMcVersionsAsync()
    {
        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync("https://meta.fabricmc.net/v2/versions/game");
            responseMessage.EnsureSuccessStatusCode();

            return JArray.Parse(await responseMessage.Content.ReadAsStringAsync()).Select(x => (string)x["version"]).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static async Task<FabricMavenItem[]> GetFabricLoaderMavensAsync()
    {
        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync("https://meta.fabricmc.net/v2/versions/loader");
            responseMessage.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<FabricMavenItem[]>(await responseMessage.Content.ReadAsStringAsync());
        }
        catch
        {
            return Array.Empty<FabricMavenItem>();
        }
    }

    public static async Task<FabricInstallBuild[]> GetFabricBuildsFromMcVersionAsync(string mcVersion)
    {
        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync($"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}");
            responseMessage.EnsureSuccessStatusCode();

            var list = JsonConvert.DeserializeObject<List<FabricInstallBuild>>(await responseMessage.Content.ReadAsStringAsync());

            list.Sort((a, b) => new Version(a.Loader.Version.Replace(a.Loader.Separator, ".")).CompareTo(new Version(b.Loader.Version.Replace(b.Loader.Separator, "."))));
            list.Reverse();

            return list.ToArray();
        }
        catch // (Exception ex)
        {
            return Array.Empty<FabricInstallBuild>();
        }
    }
}

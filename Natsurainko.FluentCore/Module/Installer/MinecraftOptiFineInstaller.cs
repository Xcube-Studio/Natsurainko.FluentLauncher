using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Install.OptiFine;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Service;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Installer;

public class MinecraftOptiFineInstaller : InstallerBase
{
    public string JavaPath { get; private set; }

    public OptiFineInstallBuild OptiFineBuild { get; private set; }

    public string PackageFile { get; private set; }

    public MinecraftOptiFineInstaller(IGameCoreLocator coreLocator, OptiFineInstallBuild build, string javaPath, string packageFile = null, string customId = null) : base(coreLocator, customId)
    {
        OptiFineBuild = build;
        JavaPath = javaPath;
        PackageFile = packageFile;
    }

    public override async Task<InstallerResponse> InstallAsync()
    {
        #region Download Package

        OnProgressChanged("Downloading OptiFine Insatller Package", 0.0f);

        if (string.IsNullOrEmpty(PackageFile) || !File.Exists(PackageFile))
        {
            var downloadResponse = await DownloadOptiFinePackageFromBuildAsync(this.OptiFineBuild, GameCoreLocator.Root, (progress, message) =>
            {
                OnProgressChanged("Downloading OptiFine Insatller Package", 0.15f * progress);
            });

            if (downloadResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new HttpRequestException(downloadResponse.HttpStatusCode.ToString());

            PackageFile = downloadResponse.FileInfo.FullName;
        }

        #endregion

        #region Parse Package

        OnProgressChanged("Parsing Installer Package", 0.45f);

        using var archive = ZipFile.OpenRead(PackageFile);
        string launchwrapper = "1.12";

        if (archive.GetEntry("launchwrapper-of.txt") != null)
            launchwrapper = archive.GetEntry("launchwrapper-of.txt").GetString();

        #endregion

        #region Check Inherited Core

        await CheckInheritedCore(0.45f, 0.60f, OptiFineBuild.McVersion);

        #endregion

        #region Write Files

        OnProgressChanged("Writing Files", 0.70f);

        var entity = new VersionJsonEntity
        {
            Id = string.IsNullOrEmpty(CustomId) ? $"{OptiFineBuild.McVersion}-OptiFine-{OptiFineBuild.Type}_{OptiFineBuild.Patch}" : CustomId,
            InheritsFrom = OptiFineBuild.McVersion,
            Time = DateTime.Now.ToString("O"),
            ReleaseTime = DateTime.Now.ToString("O"),
            Type = "release",
            Libraries = new()
            {
                new LibraryJsonEntity { Name = $"optifine:Optifine:{OptiFineBuild.McVersion}_{OptiFineBuild.Type}_{OptiFineBuild.Patch}" },
                new LibraryJsonEntity { Name = launchwrapper.Equals("1.12") ? "net.minecraft:launchwrapper:1.12" : $"optifine:launchwrapper-of:{launchwrapper}" }
            },
            MainClass = "net.minecraft.launchwrapper.Launch",
            Arguments = new()
            {
                Game = new()
                {
                    "--tweakClass",
                      "optifine.OptiFineTweaker"
                }
            },
            JavaVersion = null
        };

        var versionJsonFile = new FileInfo(Path.Combine(GameCoreLocator.Root.FullName, "versions", entity.Id, $"{entity.Id}.json"));

        if (!versionJsonFile.Directory.Exists)
            versionJsonFile.Directory.Create();

        File.WriteAllText(versionJsonFile.FullName, entity.ToJson());

        var launchwrapperFile = new LibraryResource() { Name = entity.Libraries[1].Name, Root = this.GameCoreLocator.Root }.ToFileInfo();

        if (!launchwrapper.Equals("1.12"))
        {
            if (!launchwrapperFile.Directory.Exists)
                launchwrapperFile.Directory.Create();

            archive.GetEntry($"launchwrapper-of-{launchwrapper}.jar").ExtractToFile(launchwrapperFile.FullName, true);
        }
        else if (!launchwrapperFile.Exists)
            await HttpWrapper.HttpDownloadAsync(new LibraryResource() { Name = entity.Libraries[1].Name, Root = this.GameCoreLocator.Root }.ToDownloadRequest());

        string inheritsFromFile = Path.Combine(GameCoreLocator.Root.FullName, "versions", OptiFineBuild.McVersion, $"{OptiFineBuild.McVersion}.jar");
        File.Copy(inheritsFromFile, Path.Combine(versionJsonFile.Directory.FullName, $"{entity.Id}.jar"), true);

        var optiFineLibraryFile = new LibraryResource { Name = entity.Libraries[0].Name, Root = this.GameCoreLocator.Root }.ToFileInfo();

        if (!optiFineLibraryFile.Directory.Exists)
            optiFineLibraryFile.Directory.Create();

        #endregion

        #region Run Processor

        OnProgressChanged("Running Installer Processor", 0.85f);

        using var process = Process.Start(new ProcessStartInfo(JavaPath)
        {
            UseShellExecute = false,
            WorkingDirectory = this.GameCoreLocator.Root.FullName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = string.Join(" ", new string[]
            {
                "-cp",
                PackageFile,
                "optifine.Patcher",
                inheritsFromFile,
                PackageFile,
                optiFineLibraryFile.FullName
            })
        });

        var outputs = new List<string>();
        var errorOutputs = new List<string>();

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                outputs.Add(args.Data);
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputs.Add(args.Data);
                errorOutputs.Add(args.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        if (errorOutputs.Count > 0)
        {
            OnProgressChanged("Finished", 1.0f);

            return new()
            {
                Success = false,
                GameCore = null,
                Exception = null
            };
        }
        #endregion

        #region Clean

        File.Delete(PackageFile);

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
            using var responseMessage = await HttpWrapper.HttpGetAsync($"{(DownloadApiManager.Current.Host.Equals(DownloadApiManager.Mojang.Host) ? DownloadApiManager.Bmcl.Host : DownloadApiManager.Current.Host)}/optifine/versionList");
            responseMessage.EnsureSuccessStatusCode();

            return JArray.Parse(await responseMessage.Content.ReadAsStringAsync()).Select(x => (string)x["mcversion"]).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static async Task<OptiFineInstallBuild[]> GetOptiFineBuildsFromMcVersionAsync(string mcVersion)
    {
        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync($"{(DownloadApiManager.Current.Host.Equals(DownloadApiManager.Mojang.Host) ? DownloadApiManager.Bmcl.Host : DownloadApiManager.Current.Host)}/optifine/{mcVersion}");
            responseMessage.EnsureSuccessStatusCode();

            var list = JsonConvert.DeserializeObject<List<OptiFineInstallBuild>>(await responseMessage.Content.ReadAsStringAsync());

            var preview = list.Where(x => x.Patch.StartsWith("pre")).ToList();
            var release = list.Where(x => !x.Patch.StartsWith("pre")).ToList();

            release.Sort((a, b) => $"{a.Type}_{a.Patch}".CompareTo($"{b.Type}_{b.Patch}"));
            preview.Sort((a, b) => $"{a.Type}_{a.Patch}".CompareTo($"{b.Type}_{b.Patch}"));

            var builds = preview.Union(release).ToList();
            builds.Reverse();

            return builds.ToArray();
        }
        catch
        {
            return Array.Empty<OptiFineInstallBuild>();
        }
    }

    public static Task<HttpDownloadResponse> DownloadOptiFinePackageFromBuildAsync(OptiFineInstallBuild build, DirectoryInfo directory, Action<float, string> progressChangedAction)
    {
        var downloadUrl = $"{(DownloadApiManager.Current.Host.Equals(DownloadApiManager.Mojang.Host) ? DownloadApiManager.Bmcl.Host : DownloadApiManager.Current.Host)}/optifine/{build.McVersion}/{build.Type}/{build.Patch}";
        return HttpWrapper.HttpDownloadAsync(new()
        {
            Url = downloadUrl,
            Directory = directory
        }, progressChangedAction);
    }
}

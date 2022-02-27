using FluentCore.Model;
using FluentCore.Model.Game;
using FluentCore.Model.Install.Forge;
using FluentCore.Service.Component.DependencesResolver;
using FluentCore.Service.Component.Launch;
using FluentCore.Service.Local;
using FluentCore.Service.Network;
using FluentCore.Service.Network.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FluentCore.Service.Component.Installer.ForgeInstaller
{
    /// <summary>
    /// Forge安装器
    /// <para>
    /// 1.13+
    /// </para>
    /// </summary>
    public class ModernForgeInstaller : InstallerBase
    {
        /// <summary>
        /// 游戏id
        /// </summary>
        public string McVersionId { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string McVersion { get; set; }

        /// <summary>
        /// Java可执行文件路径
        /// </summary>
        public string JavaPath { get; set; }

        /// <summary>
        /// Forge安装包位置
        /// </summary>
        public string ForgeInstallerPackagePath { get; set; }

        public List<HttpDownloadRequest> ErrorDownload = new List<HttpDownloadRequest>();

        public ModernForgeInstaller(CoreLocator locator, string mcVersion, string mcVersionId, string javaPath, string forgeInstallerPackagePath)
            : base(locator)
        {
            this.McVersion = mcVersion;
            this.McVersionId = mcVersionId;
            this.JavaPath = javaPath;
            this.ForgeInstallerPackagePath = forgeInstallerPackagePath;
        }

        /// <summary>
        /// 安装
        /// </summary>
        /// <returns></returns>
        public ForgeInstallerResultModel Install() => InstallAsync().GetAwaiter().GetResult();

        /// <summary>
        /// 安装(异步)
        /// </summary>
        /// <returns></returns
        public async Task<ForgeInstallerResultModel> InstallAsync()
        {
            using var archive = ZipFile.OpenRead(this.ForgeInstallerPackagePath);
            var processOutputs = new List<string>();
            var processErrorOutputs = new List<string>();

            #region Get version.json

            OnProgressChanged(0.1, $"Installing Forge Loader - Initializing");

            var versionJson = await ZipFileHelper.GetStringFromJsonEntryAsync
                (archive.Entries.First(x => x.Name.Equals("version.json", StringComparison.OrdinalIgnoreCase)));
            var versionModel = JsonConvert.DeserializeObject<CoreModel>(versionJson);

            var versionJsonFile = new FileInfo
                ($"{PathHelper.GetVersionFolder(this.CoreLocator.Root, versionModel.Id)}{PathHelper.X}{versionModel.Id}.json");
            if (!versionJsonFile.Directory.Exists)
                versionJsonFile.Directory.Create();

            File.WriteAllText(versionJsonFile.FullName, versionJson);

            OnProgressChanged(0.15, $"Installing Forge Loader - Initializing");

            #endregion

            #region Get install_profile.json

            var forgeInstallProfile = await ZipFileHelper.GetObjectFromJsonEntryAsync<ForgeInstallProfileModel>
                (archive.Entries.First(x => x.Name.Equals("install_profile.json", StringComparison.OrdinalIgnoreCase)));
            var forgeVersion = forgeInstallProfile.Version.Replace("-forge-", "-");

            forgeInstallProfile.Data["BINPATCH"].Client = $"[net.minecraftforge:forge:{forgeVersion}:clientdata@lzma]";
            forgeInstallProfile.Data["BINPATCH"].Server = $"[net.minecraftforge:forge:{forgeVersion}:serverdata@lzma]";

            OnProgressChanged(0.2, $"Installing Forge Loader - Initialized");

            #endregion

            #region Get Lzma

            OnProgressChanged(0.3, $"Installing Forge Loader - Extracting Files");

            var clientLzma = archive.Entries.FirstOrDefault(e => e.FullName.Equals("data/client.lzma", StringComparison.OrdinalIgnoreCase));
            var serverLzma = archive.Entries.FirstOrDefault(e => e.FullName.Equals("data/server.lzma", StringComparison.OrdinalIgnoreCase));

            var clientLzmaFile = new FileInfo(GetMavenFilePath(forgeInstallProfile.Data["BINPATCH"].Client));
            var serverLzmaFile = new FileInfo(GetMavenFilePath(forgeInstallProfile.Data["BINPATCH"].Server));

            await ZipFileHelper.WriteAsync(clientLzma, clientLzmaFile.Directory.FullName, clientLzmaFile.Name);
            await ZipFileHelper.WriteAsync(serverLzma, serverLzmaFile.Directory.FullName, serverLzmaFile.Name);

            #endregion

            #region Extract Forge Jar

            var forgeJar = archive.Entries.FirstOrDefault
                (e => e.FullName.Equals($"maven/net/minecraftforge/forge/{forgeVersion}/forge-{forgeVersion}.jar", StringComparison.OrdinalIgnoreCase));
            var forgeUniversalJar = archive.Entries.FirstOrDefault
                (e => e.FullName.Equals($"maven/net/minecraftforge/forge/{forgeVersion}/forge-{forgeVersion}-universal.jar", StringComparison.OrdinalIgnoreCase));

            if (forgeJar != default)
            {
                if (forgeUniversalJar != default)
                {
                    var fileUniversalJar = new FileInfo($"{PathHelper.GetLibrariesFolder(this.CoreLocator.Root)}{PathHelper.X}{forgeUniversalJar.FullName.Replace("maven/", "").Replace("/", PathHelper.X)}");
                    if (!fileUniversalJar.Directory.Exists)
                        fileUniversalJar.Directory.Create();

                    await ZipFileHelper.WriteAsync(forgeUniversalJar, fileUniversalJar.Directory.FullName);
                }

                var file = new FileInfo($"{PathHelper.GetLibrariesFolder(this.CoreLocator.Root)}{PathHelper.X}{forgeJar.FullName.Replace("maven/", "").Replace("/", PathHelper.X)}");
                if (!file.Directory.Exists)
                    file.Directory.Create();

                await ZipFileHelper.WriteAsync(forgeJar, file.Directory.FullName);
            }

            OnProgressChanged(0.4, $"Installing Forge Loader - Extracted");

            #endregion

            #region Parser Processor

            OnProgressChanged(0.4, $"Installing Forge Loader - Parsering Processor");

            var replaceValues = new Dictionary<string, string>
            {
                { "{SIDE}", "client" },
                { "{MINECRAFT_JAR}", $"{PathHelper.GetVersionFolder(this.CoreLocator.Root,this.McVersionId)}{PathHelper.X}{this.McVersionId}.jar" },
                { "{MINECRAFT_VERSION}", this.McVersion },
                { "{ROOT}", this.CoreLocator.Root },
                { "{INSTALLER}", this.ForgeInstallerPackagePath },
                { "{LIBRARY_DIR}", PathHelper.GetLibrariesFolder(this.CoreLocator.Root) },
                { "/",PathHelper.X }
};
            var replaceArgs = forgeInstallProfile.Data.Select(x => ($"{{{x.Key}}}", x.Value.Client)).ToDictionary(k => k.Item1, v =>
            {
                if (v.Client.Contains("[") && v.Client.Contains("]"))
                    return GetMavenFilePath(v.Client);

                return v.Client;
            });

            for (int i = 0; i < forgeInstallProfile.Processors.Count; i++)
            {
                var processModel = forgeInstallProfile.Processors[i];

                if (processModel.Sides != null && !processModel.Sides.Any(s => s.Equals("client", StringComparison.OrdinalIgnoreCase)))
                {
                    forgeInstallProfile.Processors.RemoveAt(i);
                    continue;
                }
            }

            for (int i = 0; i < forgeInstallProfile.Processors.Count; i++)
            {
                var processModel = forgeInstallProfile.Processors[i];

                processModel.Args = StringHelper.Replace(processModel.Args, replaceValues).ToList();
                processModel.Args = StringHelper.Replace(processModel.Args, replaceArgs).ToList();

                processModel.Args = processModel.Args.Select(x =>
                {
                    if (x.Contains("[") && x.Contains("]"))
                        return $"{Path.Combine(PathHelper.GetLibrariesFolder(this.CoreLocator.Root), new Library { Name = x.TrimStart('[').TrimEnd(']') }.GetRelativePath())}";

                    return x;
                }).ToList();

                if (processModel.Outputs != default)
                    processModel.Outputs = processModel.Outputs.Select
                        (x => (StringHelper.Replace(x.Key, replaceArgs), StringHelper.Replace(x.Value, replaceArgs))).ToDictionary(k => k.Item1, v => v.Item2);

                forgeInstallProfile.Processors[i] = processModel;
            }

            OnProgressChanged(0.5, $"Installing Forge Loader - Parsered Processor");

            #endregion

            #region Download Libraries

            OnProgressChanged(0.5, $"Installing Forge Loader - Downloading Libraries");

            for (int i = 0;i < versionModel.Libraries.Count;i++)
                if (versionModel.Libraries[i].Name == forgeInstallProfile.Path)
                    versionModel.Libraries.Remove(versionModel.Libraries[i]);

            for (int i = 0; i < forgeInstallProfile.Libraries.Count; i++)
                if (forgeInstallProfile.Libraries[i].Name == $"{forgeInstallProfile.Path}:universal")
                    forgeInstallProfile.Libraries.Remove(forgeInstallProfile.Libraries[i]);

            async Task Download(IEnumerable<HttpDownloadRequest> requests)
            {
                var manyBlock = new TransformManyBlock<IEnumerable<HttpDownloadRequest>, HttpDownloadRequest>(x => x);
                var blockOptions = new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = DependencesCompleter.MaxThread,
                    MaxDegreeOfParallelism = DependencesCompleter.MaxThread
                };

                var actionBlock = new ActionBlock<HttpDownloadRequest>(async x =>
                {
                    try
                    {
                        if (!x.Directory.Exists)
                            x.Directory.Create();

                        var res = await HttpHelper.HttpDownloadAsync(x, x.FileName);
                        if (res.HttpStatusCode != HttpStatusCode.OK)
                            this.ErrorDownload.Add(x);
                    }
                    catch
                    {
                        throw;
                    }
                }, blockOptions);

                var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
                _ = manyBlock.LinkTo(actionBlock, linkOptions);

                _ = manyBlock.Post(requests);
                manyBlock.Complete();

                await actionBlock.Completion;
                GC.Collect();
            }

            var downloadList = versionModel.Libraries.Union(forgeInstallProfile.Libraries).Select(x =>
            {
                var result = x.GetDownloadRequest(this.CoreLocator.Root, true);

                if (SystemConfiguration.Api.Url != new Mojang().Url)
                    result.Url = result.Url.Replace("https://maven.minecraftforge.net", $"{SystemConfiguration.Api.Url}/maven");

                return result;
            });

            await Download(downloadList);

            //Try Again
            if (ErrorDownload.Count > 0)
                await Download(ErrorDownload.Select(x =>
                {
                    x.Url = x.Url.Replace($"{SystemConfiguration.Api.Url}/maven", "https://maven.minecraftforge.net");
                    return x;
                }));

            OnProgressChanged(0.8, $"Installing Forge Loader - Downloaded Libraries");

            #endregion

            #region Run Process

            OnProgressChanged(0.8, $"Installing Forge Loader - Processing");

            foreach (var process in forgeInstallProfile.Processors)
            {
                string JarFile = $"{Path.Combine(PathHelper.GetLibrariesFolder(this.CoreLocator.Root), new Library { Name = process.Jar }.GetRelativePath())}";

                using var JarFileArchive = ZipFile.OpenRead(JarFile);

                var libEntry = JarFileArchive.Entries.FirstOrDefault(e => e.FullName.Equals("META-INF/MANIFEST.MF", StringComparison.OrdinalIgnoreCase));
                string[] lines = (await ZipFileHelper.GetStringFromJsonEntryAsync(libEntry)).Split("\r\n");
                var mainClass = lines.ToList().FirstOrDefault(x => x.Contains("Main-Class: ")).Replace("Main-Class: ", "");

                var libs = new List<string>() { process.Jar }.Concat(process.Classpath);
                var classpath = libs.Select(x => $"{Path.Combine(PathHelper.GetLibrariesFolder(this.CoreLocator.Root), new Library { Name = x }.GetRelativePath())}");
                var parameter = new List<string>
                {
                    "-cp",
                    $"\"{string.Join(ArgumentsBuilder.Separator,classpath)}\"",
                    mainClass
                };

                parameter.AddRange(process.Args);

                var processContainer = new ProcessContainer(new ProcessStartInfo(JavaPath)
                {
                    Arguments = string.Join(" ", parameter),
                    WorkingDirectory = this.CoreLocator.Root
                });

                processContainer.Start();
                await processContainer.Process.WaitForExitAsync();

                processOutputs.AddRange(processContainer.OutputData);
                processErrorOutputs.AddRange(processContainer.ErrorData);

                processContainer.Dispose();
            }

            OnProgressChanged(0.9, $"Installing Forge Loader - Processed");

            #endregion

            OnProgressChanged(1.0, $"Installing Forge Loader - Finished");

            if (processErrorOutputs.Count > 0)
                return new ForgeInstallerResultModel
                {
                    IsSuccessful = false,
                    Message = $"Failed Install Forge-{forgeVersion}!",
                    ProcessOutput = processOutputs,
                    ProcessErrorOutput = processErrorOutputs
                };

            return new ForgeInstallerResultModel
            {
                IsSuccessful = true,
                Message = $"Successfully Install Forge-{forgeVersion}!",
                ProcessOutput = processOutputs,
                ProcessErrorOutput = processErrorOutputs
            };
        }

        private string GetMavenFilePath(string maven)
        {
            string[] values = maven.TrimStart('[').TrimEnd(']').Split(":");

            string path = $"{PathHelper.GetLibrariesFolder(this.CoreLocator.Root)}" +
                $"/{values[0].Replace(".", "/")}" +
                $"/{values[1]}" +
                $"/{values[2]}" +
                $"/{values[1]}-{values[2]}-{values[3].Replace("@", ".")}";

            if (!maven.Contains("@"))
                path += ".jar";

            return path.Replace("/", PathHelper.X).Replace("\\", PathHelper.X);
        }
    }
}

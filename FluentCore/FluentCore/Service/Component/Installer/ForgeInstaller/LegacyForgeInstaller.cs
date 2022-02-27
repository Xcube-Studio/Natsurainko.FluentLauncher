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
    /// 1.12-
    /// </para>
    /// </summary>
    public class LegacyForgeInstaller : InstallerBase
    {
        /// <summary>
        /// Forge安装包位置
        /// </summary>
        public string ForgeInstallerPackagePath { get; set; }

        public List<HttpDownloadRequest> ErrorDownload = new List<HttpDownloadRequest>();

        public LegacyForgeInstaller(CoreLocator locator, string forgeInstallerPackagePath)
            : base(locator)
        {
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
        /// <returns></returns>
        public async Task<ForgeInstallerResultModel> InstallAsync()
        {
            using var archive = ZipFile.OpenRead(this.ForgeInstallerPackagePath);

            #region Get install_profile.json

            OnProgressChanged(0.1, $"Installing Forge Loader - Initializing");

            var forgeInstallProfile = await ZipFileHelper.GetObjectFromJsonEntryAsync<LegacyForgeInstallProfileModel>
                (archive.Entries.First(x => x.Name.Equals("install_profile.json", StringComparison.OrdinalIgnoreCase)));

            #endregion

            #region Get version.json

            var versionJsonFile = new FileInfo
                ($"{PathHelper.GetVersionFolder(this.CoreLocator.Root, forgeInstallProfile.Install.Target)}{PathHelper.X}{forgeInstallProfile.Install.Target}.json");
            if (!versionJsonFile.Directory.Exists)
                versionJsonFile.Directory.Create();

            File.WriteAllText(versionJsonFile.FullName, forgeInstallProfile.VersionInfo.ToString());

            OnProgressChanged(0.2, $"Installing Forge Loader - Initialized");

            #endregion

            #region Extract Forge Jar

            OnProgressChanged(0.3, $"Installing Forge Loader - Extracting Files");

            var legacyJarEntry = archive.Entries.FirstOrDefault(e => e.Name.Equals(forgeInstallProfile.Install.FilePath, StringComparison.OrdinalIgnoreCase));
            var model = JsonConvert.DeserializeObject<CoreModel>(forgeInstallProfile.VersionInfo.ToString());

            var lib = model.Libraries.First(x => x.Name.StartsWith("net.minecraftforge:forge", StringComparison.OrdinalIgnoreCase));
            var fileLib = new FileInfo(Path.Combine(PathHelper.GetLibrariesFolder(this.CoreLocator.Root), lib.GetRelativePath()));
            await ZipFileHelper.WriteAsync(legacyJarEntry, fileLib.Directory.FullName, fileLib.Name);

            OnProgressChanged(0.4, $"Installing Forge Loader - Extracted");

            #endregion

            #region Download Libraries

            OnProgressChanged(0.5, $"Installing Forge Loader - Downloading Libraries");

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

            var downloadList = model.Libraries.Select(x => x.GetDownloadRequest(this.CoreLocator.Root)).ToList();
            downloadList.Remove(downloadList.First(x => x.Url.Equals(lib.GetDownloadRequest(this.CoreLocator.Root).Url)));

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

            OnProgressChanged(1.0, $"Installing Forge Loader - Finished");

            return new ForgeInstallerResultModel
            {
                IsSuccessful = true,
                Message = $"Successfully Install {forgeInstallProfile.Install.Target}!"
            };
        }
    }
}

using FluentCore.Model.Game;
using FluentCore.Model.Install.OptiFine;
using FluentCore.Service.Component.Launch;
using FluentCore.Service.Local;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.Installer
{
    /// <summary>
    /// OptiFine安装器
    /// </summary>
    public class OptiFineInstaller : InstallerBase
    {
        /// <summary>
        /// OptiFine安装包位置
        /// </summary>
        public string OptiFineInstallerPackagePath { get; set; }

        /// <summary>
        /// Java可执行文件路径
        /// </summary>
        public string JavaPath { get; set; }

        /// <summary>
        /// launchwrapper 版本
        /// 在1.8.8及以下版本中为常量，且恒为"1.12"
        /// </summary>
        public string Launchwrapper { get; private set; } = "1.12";

        /// <summary>
        /// 版本号
        /// </summary>
        public string McVersion { get; set; }

        /// <summary>
        /// 游戏id
        /// </summary>
        public string McVersionId { get; set; }

        public OptiFineInstaller(CoreLocator locator, string mcVersion, string mcVersionId, string javaPath, string optiFineInstallerPackagePath)
            :base(locator)
        {
            this.OptiFineInstallerPackagePath = optiFineInstallerPackagePath;
            this.McVersion = mcVersion;
            this.McVersionId = mcVersionId;
            this.JavaPath = javaPath;
        }

        /// <summary>
        /// 安装(异步)
        /// </summary>
        /// <returns></returns
        public async Task<OptiFineInstallerResultModel> InstallAsync()
        {
            #region Initialize
            using var archive = ZipFile.OpenRead(OptiFineInstallerPackagePath);
            using var stream = archive.GetEntry("changelog.txt").Open();
            using var streamReader = new StreamReader(stream);
            string[] info = (await streamReader.ReadLineAsync()).Split(" ");
            string path = info[1].Replace($"{info[1].Split("_")[0]}_", string.Empty);

            var result = new OptiFineInstallerResultModel
            {
                IsSuccessful = true
            };

            foreach (var entry in archive.Entries)
                if (entry.Name.Equals("launchwrapper-of.txt"))
                {
                    using var entryStream = entry.Open();
                    using var reader = new StreamReader(entryStream);
                    Launchwrapper = reader.ReadToEnd();
                }

            var versionModel = new CoreModel
            {
                Id = $"{McVersion}-OptiFine_{path}",
                InheritsFrom = McVersion,
                ReleaseTime = DateTime.Now.ToString("yyyy-MM-dd{a}hh:mm:ss{b}zzz").Replace("{a}", "T").Replace("{b}", ""),
                Time = DateTime.Now.ToString("yyyy-MM-dd{a}hh:mm:ss{b}zzz").Replace("{a}", "T").Replace("{b}", ""),
                Type = "release",
                Libraries = new List<Library>
                {
                    new()
                    {
                        Name = $"optifine:Optifine:{McVersion}_{path}"
                    },
                    new()
                    {
                        Name = Launchwrapper == "1.12" ? "net.minecraft:launchwrapper:1.12" : $"optifine:launchwrapper-of:{Launchwrapper}"
                    }
                },
                MainClass = "net.minecraft.launchwrapper.Launch"
            };
            versionModel.JavaVersion = null;

            if (Convert.ToInt32(McVersion.Split('.')[1]) < 13)
            {
                var rawCore = CoreLocator.GetCoreModelFromId(McVersionId);
                versionModel.MinecraftArguments = $"{rawCore.MinecraftArguments} --tweakClass optifine.OptiFineTweaker";
            }
            else versionModel.Arguments = new Arguments
            {
                Game = new List<object>
                    {
                        "--tweakClass",
                        "optifine.OptiFineTweaker"
                    }
            };

            #endregion

            #region Write

            string inheritsFrom = Path.Combine(CoreLocator.Root, "versions", McVersionId, $"{McVersionId}.jar");

            string versionFolder = Path.Combine(CoreLocator.Root, "versions", $"{McVersion}-OptiFine_{path}");
            if (!Directory.Exists(versionFolder))
                Directory.CreateDirectory(versionFolder);

            await File.WriteAllTextAsync(Path.Combine(versionFolder, $"{McVersion}-OptiFine_{path}.json"),
                JsonConvert.SerializeObject(versionModel, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                }));

            File.Copy(inheritsFrom, Path.Combine(versionFolder, $"{McVersion}-OptiFine_{path}.jar"), true);

            if (Launchwrapper != "1.12")
            {
                var launcherwrapperEntry = archive.GetEntry($"launchwrapper-of-{Launchwrapper}.jar");
                var file = new FileInfo(Path.Combine(CoreLocator.Root, "libraries", versionModel.Libraries[1].GetRelativePath()));
                if (!file.Directory.Exists)
                    file.Directory.Create();

                launcherwrapperEntry.ExtractToFile(file.FullName, true);
            }

            #endregion

            #region Process

            var launchwrapperJar = new FileInfo(Path.Combine(CoreLocator.Root, "libraries", versionModel.Libraries[0].GetRelativePath()));
            if(!launchwrapperJar.Directory.Exists)
                launchwrapperJar.Directory.Create();

            var processContainer = new ProcessContainer(new ProcessStartInfo
            {
                FileName = JavaPath,
                ArgumentList = 
                {
                    "-cp",
                    OptiFineInstallerPackagePath,
                    "optifine.Patcher",
                    inheritsFrom,
                    OptiFineInstallerPackagePath,
                    launchwrapperJar.FullName
                }
            });

            processContainer.Start();
            await processContainer.Process.WaitForExitAsync();
            result.ProcessErrorOutput = processContainer.ErrorData;
            result.ProcessOutput = processContainer.OutputData;

            if (processContainer.ErrorData.Count > 0)
            {
                result.Message = $"Failed Install {versionModel.Id}!";
                result.IsSuccessful = false;
            }

            processContainer.Dispose();

            #endregion

            result.Message = $"Successfully Install {versionModel.Id}!";

            return result;
        }

        /// <summary>
        /// 安装
        /// </summary>
        /// <returns></returns
        public OptiFineInstallerResultModel Install() => InstallAsync().GetAwaiter().GetResult();
    }
}

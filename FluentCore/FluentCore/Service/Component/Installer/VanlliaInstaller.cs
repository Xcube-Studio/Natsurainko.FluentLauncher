using FluentCore.Service.Component.DependencesResolver;
using FluentCore.Service.Component.Launch;
using FluentCore.Service.Local;
using FluentCore.Service.Network;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.Installer
{
    /// <summary>
    /// 原版游戏安装器
    /// </summary>
    public class VanlliaInstaller : InstallerBase
    {
        public VanlliaInstaller(CoreLocator locator) : base(locator) { }

        /// <summary>
        /// 根据版本号安装对应的游戏
        /// </summary>
        /// <param name="mcVersion">版本号</param>
        /// <returns></returns>
        public async Task<bool> InstallAsync(string mcVersion)
        {
            try
            {
                foreach (var item in (await SystemConfiguration.Api.GetVersionManifest()).Versions)
                {
                    if (item.Id == mcVersion)
                    {
                        var directory = new DirectoryInfo(PathHelper.GetVersionFolder(this.CoreLocator.Root, mcVersion));

                        if (!directory.Exists)
                            directory.Create();

                        var res = await HttpHelper.HttpDownloadAsync(item.Url, directory.FullName);
                        if (res.HttpStatusCode != HttpStatusCode.OK)
                            return false;

                        OnProgressChanged(0.1, $"Installing Vanllia Minecraft - Downloaded {res.FileInfo.Name}");

                        var completer = new DependencesCompleter(this.CoreLocator.GetGameCoreFromId(mcVersion));
                        int done = 0;
                        completer.SingleDownloadedEvent += delegate (object sender, Model.HttpDownloadResponse e)
                        {
                            done++;
                            double progress = 0.1 + (done / ((double)completer.NeedDownloadDependencesCount) * 0.9);

                            OnProgressChanged(progress, string.Format("Installing Vanllia Minecraft - Downloaded {0} {1}/{2}", progress.ToString("P", new NumberFormatInfo
                            {
                                PercentDecimalDigits = 1,
                                PercentPositivePattern = 1
                            }), done, completer.NeedDownloadDependencesCount));
                        };

                        await completer.CompleteAsync();

                        OnProgressChanged(1.0, $"Installing Vanllia Minecraft - Finished");

                        return true;
                    }
                }
            }
            catch 
            {
                OnProgressChanged(1.0, $"Installing Vanllia Minecraft Core - Finished");
                //throw; 
            }
            return false;
        }

        public bool Install(string mcVersion) => InstallAsync(mcVersion).GetAwaiter().GetResult();
    }
}

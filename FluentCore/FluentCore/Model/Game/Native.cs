using FluentCore.Service.Local;
using System.IO;

namespace FluentCore.Model.Game
{
    /// <summary>
    /// 游戏Native(继承于Library)依赖
    /// </summary>
    public class Native : Library
    {
        public Native(Library library)
        {
            this.CheckSums = library.CheckSums;
            this.ClientReq = library.ClientReq;
            this.Downloads = library.Downloads;
            this.Name = library.Name;
            this.Natives = library.Natives;
            this.Rules = library.Rules;
            this.ServerReq = library.ServerReq;
            this.Url = library.Url;
        }

        public override HttpDownloadRequest GetDownloadRequest(string root)
        {
            var file = this.Downloads.Classifiers[Natives[SystemConfiguration.PlatformName.ToLower()].Replace("${arch}", SystemConfiguration.Arch)];

            return new HttpDownloadRequest
            {
                Sha1 = file.Sha1,
                Size = file.Size,
                Url = $"{SystemConfiguration.Api.Libraries}/{this.GetRelativePath().Replace("\\", "/")}",
                Directory = new FileInfo($"{PathHelper.GetLibrariesFolder(root)}{PathHelper.X}{this.GetRelativePath()}").Directory,
                FileName = Path.GetFileName(GetRelativePath())
            };
        }

        public override string GetRelativePath()
        {
            string[] temp = Name.Split(':');
            return $"{temp[0].Replace(".", PathHelper.X)}{PathHelper.X}{temp[1]}{PathHelper.X}{temp[2]}{PathHelper.X}" +
                $"{temp[1]}-{temp[2]}-{Natives[SystemConfiguration.PlatformName.ToLower()].Replace("${arch}", SystemConfiguration.Arch)}.jar";
        }
    }
}

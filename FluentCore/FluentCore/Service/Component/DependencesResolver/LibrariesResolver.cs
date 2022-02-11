using FluentCore.Interface;
using FluentCore.Model.Launch;
using FluentCore.Service.Local;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.DependencesResolver
{
    /// <summary>
    /// Library检索器
    /// </summary>
    public class LibrariesResolver : IDependencesResolver
    {
        public GameCore GameCore { get; set; }

        public LibrariesResolver(GameCore core) => this.GameCore = core;

        public IEnumerable<IDependence> GetDependences()
        {
            foreach (var lib in this.GameCore.Libraries)
                yield return lib;
            foreach (var native in this.GameCore.Natives)
                yield return native;
        }

        public IEnumerable<IDependence> GetLostDependences()
        {
            foreach (var lib in this.GameCore.Libraries)
            {
                var file = new FileInfo($"{PathHelper.GetLibrariesFolder(GameCore.Root)}{PathHelper.X}{lib.GetRelativePath()}");

                if (!file.Exists)
                {
                    yield return lib;
                    continue;
                }

                if (lib.Downloads != null && lib.Downloads.Artifact != null && !FileHelper.FileVerify(file, lib.Downloads.Artifact.Size, lib.Downloads.Artifact.Sha1))
                    yield return lib;
            }

            foreach (var native in this.GameCore.Natives)
            {
                var file = new FileInfo($"{PathHelper.GetLibrariesFolder(GameCore.Root)}{PathHelper.X}{native.GetRelativePath()}");
                var model = native.Downloads.Classifiers[native.Natives[SystemConfiguration.PlatformName.ToLower()].Replace("${arch}", SystemConfiguration.Arch)];

                if (!file.Exists)
                {
                    yield return native;
                    continue;
                }

                if (!FileHelper.FileVerify(file, model.Size, model.Sha1))
                    yield return native;
            }
        }

        public Task<IEnumerable<IDependence>> GetDependencesAsync() => Task.Run(GetDependences);

        public Task<IEnumerable<IDependence>> GetLostDependencesAsync() => Task.Run(GetLostDependences);
    }
}

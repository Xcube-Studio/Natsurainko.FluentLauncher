using FluentCore.Interface;
using FluentCore.Model.Game;
using FluentCore.Model.Launch;
using FluentCore.Service.Local;
using FluentCore.Service.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.DependencesResolver
{
    /// <summary>
    /// Asset检索器
    /// </summary>
    public class AssetsResolver : IDependencesResolver
    {
        public GameCore GameCore { get; set; }

        public AssetsResolver(GameCore core) => this.GameCore = core;

        public async Task<IEnumerable<IDependence>> GetDependencesAsync()
        {
            string path = $"{PathHelper.GetAssetIndexFolder(this.GameCore.Root)}{PathHelper.X}{this.GameCore.AsstesIndex.Id}.json";
            if (!File.Exists(path))
            {
                var res = await HttpHelper.HttpDownloadAsync(this.GameCore.AsstesIndex.Url, PathHelper.GetAssetIndexFolder(this.GameCore.Root));
                if (res.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception();
            }

            var objects = JsonConvert.DeserializeObject<AssetsObjectsModel>(File.ReadAllText(path));
            var list = new List<IDependence>();
            foreach (var asset in objects.Objects)
                list.Add(asset.Value);

            return list;
        }

        public async Task<IEnumerable<IDependence>> GetLostDependencesAsync()
        {
            var list = new List<IDependence>();

            Parallel.ForEach(await GetDependencesAsync(), x =>
            {
                var asset = (Asset)x;
                var file = new FileInfo($"{PathHelper.GetAssetsFolder(GameCore.Root)}{PathHelper.X}{asset.GetRelativePath()}");
                if (!file.Exists)
                {
                    list.Add(asset);
                    return;
                }

                if (!FileHelper.FileVerify(file, asset.Size, asset.Hash))
                    list.Add(asset);
            });

            return list;
        }

        public IEnumerable<IDependence> GetDependences() => GetDependencesAsync().GetAwaiter().GetResult();

        public IEnumerable<IDependence> GetLostDependences() => GetLostDependencesAsync().GetAwaiter().GetResult();
    }
}

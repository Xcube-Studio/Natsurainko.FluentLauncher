using FluentCore.Model;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace FluentCore.Service.Network.Api
{
    public class Mcbbs : BaseApi
    {
        public Mcbbs()
        {
            Url = "https://download.mcbbs.net";
            VersionManifest = $"{Url}/mc/game/version_manifest.json";
            Assets = $"{Url}/assets";
            Libraries = $"{Url}/maven";
        }

        public override async Task<VersionManifestModel> GetVersionManifest()
        {
            using var res = await HttpHelper.HttpGetAsync(this.VersionManifest);
            var model = JsonConvert.DeserializeObject<VersionManifestModel>(await res.Content.ReadAsStringAsync());

            var list = model.Versions.ToList();
            for (int i = 0; i < list.Count; i++)
                list[i].Url = list[i].Url.Replace("https://launchermeta.mojang.com", this.Url);

            model.Versions = list;
            return model;
        }
    }
}

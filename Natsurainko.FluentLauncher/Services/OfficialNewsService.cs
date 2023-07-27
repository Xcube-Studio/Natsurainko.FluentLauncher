using Natsurainko.FluentLauncher.Classes.Data.UI;
using Nrk.FluentCore.Utils;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services;

internal class OfficialNewsService
{
    public async Task<NewsContentData[]> GetOfficialNews()
    {
        using var res = await Task.Run(() => HttpUtils.HttpGet("https://launchercontent.mojang.com/news.json"));

        return JsonNode.Parse(await res.Content.ReadAsStringAsync())["entries"].AsArray()
            .Select(x =>
            {
                var contentData = x.Deserialize<NewsContentData>();
                contentData.ImageUrl = $"https://launchercontent.mojang.com{x["newsPageImage"]["url"].GetValue<string>()}";
                return contentData;
            }).ToArray();
    }
}

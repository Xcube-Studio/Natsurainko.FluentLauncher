using Natsurainko.FluentLauncher.Services.Data;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services;

internal class OfficialNewsService
{
    public async Task<NewsContentData[]> GetOfficialNews()
    {
        var modelType = new
        {
            title = string.Empty,
            date = string.Empty,
            text = string.Empty,
            tag = string.Empty,
            readMoreLink = string.Empty,
            newsPageImage = new
            {
                url = string.Empty
            }
        };

        using var res = await HttpWrapper.HttpGetAsync("https://launchercontent.mojang.com/news.json");

        return ((JArray)JObject.Parse(await res.Content.ReadAsStringAsync())["entries"])
            .Select(x =>
            {
                var model = JsonConvert.DeserializeAnonymousType(x.ToString(), modelType);
                return new NewsContentData(
                    $"https://launchercontent.mojang.com{model.newsPageImage.url}",
                    model.title,
                    model.tag,
                    model.date,
                    model.text,
                    model.readMoreLink);
            }).ToArray();
    }
}

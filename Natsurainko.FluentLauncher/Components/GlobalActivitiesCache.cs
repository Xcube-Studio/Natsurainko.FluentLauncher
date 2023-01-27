using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components;

public static class GlobalActivitiesCache
{
    public static List<LaunchArrangement> LaunchArrangements { get; private set; } = new List<LaunchArrangement>();

    public static List<DownloadArrangement> DownloadArrangements { get; private set; } = new List<DownloadArrangement>();

    public static List<NewsData> MojangNews { get; private set; }

    public static async Task BeginDownloadNews()
    {
        using var res = await HttpWrapper.HttpGetAsync("https://launchercontent.mojang.com/news.json");

        MojangNews = ((JArray)JObject.Parse(await res.Content.ReadAsStringAsync())["entries"])
            .Select(x => x.ToObject<NewsData>())
            .Take(25)
            .ToList();
    }
}

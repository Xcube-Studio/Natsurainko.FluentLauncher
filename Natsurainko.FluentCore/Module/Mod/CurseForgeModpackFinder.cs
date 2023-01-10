using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Mod;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Mod;

public class CurseForgeModpackFinder
{
    public string AccessToken { get; set; }

    public const int GameId = 432;

    public const string CurseForgeApi = "https://api.curseforge.com/v1/mods";

    private Dictionary<string, string> Headers => new()
    {
        { "x-api-key", AccessToken }
    };

    public CurseForgeModpackFinder(string accessToken)
    {
        AccessToken = accessToken;
    }

    public async Task<List<CurseForgeModpack>> SearchModpacksAsync(string searchFilter, ModLoaderType modLoaderType = 0, string gameVersion = null, int category = -1)
    {
        var builder = new StringBuilder(CurseForgeApi)
            .Append($"/search?gameId={GameId}")
            .Append(string.IsNullOrEmpty(searchFilter) ? string.Empty : $"&searchFilter={searchFilter}")
            .Append($"&modLoaderType={(int)modLoaderType}")
            .Append(string.IsNullOrEmpty(gameVersion) ? string.Empty : $"&gameVersion={gameVersion}")
            .Append(category == -1 ? string.Empty : $"&categoryId={gameVersion}");

        var result = new List<CurseForgeModpack>();

        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync(builder.ToString(), Headers);
            responseMessage.EnsureSuccessStatusCode();

            var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
            ((JArray)entity["data"]).ToList().ForEach(x => result.Add(ParseCurseForgeModpack((JObject)x)));

            result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

            return result;
        }
        catch { }

        return null;
    }

    public async Task<List<CurseForgeModpack>> GetModpacksAsync(int[] modIds)
    {
        var result = new List<CurseForgeModpack>();

        try
        {
            using var responseMessage = await HttpWrapper.HttpPostAsync(CurseForgeApi, new JObject { { "modIds", JToken.FromObject(modIds) } }.ToString(), Headers);
            responseMessage.EnsureSuccessStatusCode();

            var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());

            foreach (JObject jObject in ((JArray)entity["data"]).Cast<JObject>())
                result.Add(ParseCurseForgeModpack(jObject));

            result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

            return result;
        }
        catch { }

        return null;
    }

    public async Task<CurseForgeModpack> GetModpackAsync(int modId)
    {
        var builder = new StringBuilder(CurseForgeApi)
            .Append($"/{modId}");

        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync(builder.ToString(), Headers);
            responseMessage.EnsureSuccessStatusCode();

            var test = await responseMessage.Content.ReadAsStringAsync();

            return ParseCurseForgeModpack((JObject)JObject.Parse(test)["data"]);
        }
        catch { }

        return null;
    }

    public async Task<List<CurseForgeModpack>> GetFeaturedModpacksAsync()
    {
        var result = new List<CurseForgeModpack>();

        var content = new JObject
        {
            { "gameId", GameId },
            { "excludedModIds", JToken.FromObject(new int[] { 0 }) },
            { "gameVersionTypeId", null }
        };

        try
        {
            using var responseMessage = await HttpWrapper.HttpPostAsync($"{CurseForgeApi}/featured", content.ToString(), Headers);
            responseMessage.EnsureSuccessStatusCode();

            var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());

            foreach (JObject jObject in ((JArray)entity["data"]["popular"]).Cast<JObject>())
                result.Add(ParseCurseForgeModpack(jObject));

            foreach (JObject jObject in ((JArray)entity["data"]["recentlyUpdated"]).Cast<JObject>())
                result.Add(ParseCurseForgeModpack(jObject));

            foreach (JObject jObject in ((JArray)entity["data"]["featured"]).Cast<JObject>())
                result.Add(ParseCurseForgeModpack(jObject));

            result.Sort((a, b) => a.GamePopularityRank.CompareTo(b.GamePopularityRank));

            return result;
        }
        catch { }

        return null;
    }

    public async Task<List<CurseForgeModpackCategory>> GetCategories()
    {
        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync($"https://api.curseforge.com/v1/categories?gameId={GameId}", Headers);
            responseMessage.EnsureSuccessStatusCode();

            var entity = JObject.Parse(await responseMessage.Content.ReadAsStringAsync());

            return ((JArray)entity["data"]).Select(x => x.ToObject<CurseForgeModpackCategory>()).ToList();
        }
        catch { }

        return null;
    }

    public async Task<string> GetModDescriptionAsync(int modId)
    {
        var builder = new StringBuilder(CurseForgeApi)
            .Append($"/{modId}/description");

        try
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync(builder.ToString(), Headers);
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsStringAsync();
        }
        catch { }

        return null;
    }

    private CurseForgeModpack ParseCurseForgeModpack(JObject entity)
    {
        var modpack = entity.ToObject<CurseForgeModpack>();

        if (entity.ContainsKey("logo") && entity["logo"].Type != JTokenType.Null)
            modpack.IconUrl = (string)entity["logo"]["url"];

        modpack.LatestFilesIndexes.ForEach(x =>
        {
            x.DownloadUrl = $"https://edge.forgecdn.net/files/{x.FileId.ToString().Insert(4, "/")}/{x.FileName}";

            if (!modpack.Files.ContainsKey(x.SupportedVersion))
                modpack.Files.Add(x.SupportedVersion, new());

            modpack.Files[x.SupportedVersion].Add(x);
        });

        modpack.Links.Where(x => string.IsNullOrEmpty(x.Value)).Select(x => x.Key).ToList().ForEach(x => modpack.Links.Remove(x));
        modpack.Files = modpack.Files.OrderByDescending(x => (int)(float.Parse(x.Key.Substring(2)) * 100)).ToDictionary(x => x.Key, x => x.Value);
        modpack.SupportedVersions = modpack.Files.Keys.ToArray();

        return modpack;
    }
}

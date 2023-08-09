using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace Nrk.FluentCore.DefaultComponents.Download;

public class CurseForgeClient
{
    public const string Host = "https://api.curseforge.com/v1/";
    public const int MinecraftGameId = 432;

    public required string ApiKey { get; init; }

    public required int GameId { get; init; }

    private Dictionary<string, string> Header => new() { { "x-api-key", ApiKey } };

    public IEnumerable<CurseResource> SearchResources(
        string searchFilter,
        CurseResourceType? resourceType = default,
        string version = default)
    {
        var stringBuilder = new StringBuilder(Host);
        stringBuilder.Append($"mods/search?gameId={GameId}");
        stringBuilder.Append($"&sortField=Featured");
        stringBuilder.Append($"&sortOrder=desc");

        if (resourceType != null)
            stringBuilder.Append($"&categoryId=0&classId={(int)resourceType}");

        stringBuilder.Append($"&searchFilter={HttpUtility.UrlEncode(searchFilter)}");

        using var responseMessage = HttpUtils.HttpGet(stringBuilder.ToString(), Header);
        responseMessage.EnsureSuccessStatusCode();

        return JsonNode.Parse(responseMessage.Content.ReadAsString())["data"].AsArray().Select(ParseFromJsonNode);
    }

    public void GetFeaturedResources(out IEnumerable<CurseResource> mcMods, out IEnumerable<CurseResource> modPacks)
    {
        using var responseMessage = HttpUtils.HttpPost(
            Host + "mods/featured",
            JsonSerializer.Serialize(new { gameId = 432 }),
            Header);

        responseMessage.EnsureSuccessStatusCode();

        var json = JsonNode.Parse(responseMessage.Content.ReadAsString())["data"];

        var _mcMods = new List<CurseResource>();
        var _modPacks = new List<CurseResource>();

        var resources = json["featured"].AsArray().Union(json["popular"].AsArray());

        foreach (var node in resources)
        {
            var classId = node["classId"].GetValue<int>();

            if (classId.Equals((int)CurseResourceType.ModPack))
                _modPacks.Add(ParseFromJsonNode(node));
            else if (classId.Equals((int)CurseResourceType.McMod))
                _mcMods.Add(ParseFromJsonNode(node));
        }

        mcMods = _mcMods;
        modPacks = _modPacks;
    }

    public string GetResourceDescription(int resourceId)
    {
        using var responseMessage = HttpUtils.HttpGet(Host + $"mods/{resourceId}/description", Header);
        return JsonNode.Parse(responseMessage.Content.ReadAsString())["data"].GetValue<string>();
    }

    public string GetRawJsonSearchResources(string searchFilter, CurseResourceType? resourceType = default)
    {
        var stringBuilder = new StringBuilder(Host);
        stringBuilder.Append($"mods/search?gameId={GameId}");
        stringBuilder.Append($"&sortField=Featured");
        stringBuilder.Append($"&sortOrder=desc");

        if (resourceType != null)
            stringBuilder.Append($"&categoryId=0&classId={(int)resourceType}");

        stringBuilder.Append($"&searchFilter={HttpUtility.UrlEncode(searchFilter)}");

        using var responseMessage = HttpUtils.HttpGet(stringBuilder.ToString(), Header);
        responseMessage.EnsureSuccessStatusCode();

        return responseMessage.Content.ReadAsString();
    }

    public string GetRawJsonCategories()
    {
        using var responseMessage = HttpUtils.HttpGet(Host + $"categories?gameId={GameId}", Header);
        return responseMessage.Content.ReadAsString();
    }

    private CurseResource ParseFromJsonNode(JsonNode jsonNode)
    {
        var curseResource = jsonNode.Deserialize<CurseResource>();

        curseResource.WebLink = jsonNode["links"]?["websiteUrl"]?.GetValue<string>();
        curseResource.IconUrl = jsonNode["logo"]?["url"]?.GetValue<string>();
        curseResource.Authors = jsonNode["authors"]?.AsArray().Select(x => x["name"].GetValue<string>());
        curseResource.ScreenshotUrls = jsonNode["screenshots"]?.AsArray().Select(x => x["url"].GetValue<string>());
        curseResource.Categories = jsonNode["categories"]?.AsArray().Select(x => x["name"].GetValue<string>());

        return curseResource;
    }
}

using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nrk.FluentCore.DefaultComponents.Download;

public class ModrinthClient
{
    public const string Host = "https://api.modrinth.com/v2/";

    public IEnumerable<ModrinthResource> SearchResources(
        string query,
        ModrinthResourceType? resourceType = default,
        string version = default)
    {
        var stringBuilder = new StringBuilder(Host);
        stringBuilder.Append($"search?query={query}");

        var facets = new List<string>();

        if (resourceType != null)
            facets.Add($"[\"project_type:{resourceType switch
            {
                ModrinthResourceType.ModPack => "modpack",
                ModrinthResourceType.Resourcepack => "resourcepack",
                _ => "mod"
            }}\"]");

        if (version != null) facets.Add($"\"[versions:{version}\"]");
        if (facets.Any()) stringBuilder.Append($"&facets=[{string.Join(',', facets)}]");

        using var responseMessage = HttpUtils.HttpGet(stringBuilder.ToString());
        responseMessage.EnsureSuccessStatusCode();

        return JsonNode.Parse(responseMessage.Content.ReadAsString())["hits"].Deserialize<IEnumerable<ModrinthResource>>();
    }

    public string GetResourceDescription(string id)
    {
        using var responseMessage = HttpUtils.HttpGet(Host + $"project/{id}");
        responseMessage.EnsureSuccessStatusCode();

        return JsonNode.Parse(responseMessage.Content.ReadAsString())["body"].GetValue<string>();
    }

    public string GetRawJsonSearchResources(
        string query,
        ModrinthResourceType? resourceType = default,
        string version = default)
    {
        var stringBuilder = new StringBuilder(Host);
        stringBuilder.Append($"search?query={query}");

        var facets = new List<string>();

        if (resourceType != null)
            facets.Add($"[\"project_type:{resourceType switch
            {
                ModrinthResourceType.ModPack => "modpack",
                ModrinthResourceType.Resourcepack => "resourcepack",
                _ => "mod"
            }}\"]");

        if (version != null) facets.Add($"\"[versions:{version}\"]");
        if (facets.Any()) stringBuilder.Append($"&facets=[{string.Join(',', facets)}]");

        using var responseMessage = HttpUtils.HttpGet(stringBuilder.ToString());
        responseMessage.EnsureSuccessStatusCode();

        return responseMessage.Content.ReadAsString();
    }

    public string GetRawJsonGetProject(string id)
    {
        using var responseMessage = HttpUtils.HttpGet(Host + $"project/{id}");
        responseMessage.EnsureSuccessStatusCode();

        return responseMessage.Content.ReadAsString();
    }
}

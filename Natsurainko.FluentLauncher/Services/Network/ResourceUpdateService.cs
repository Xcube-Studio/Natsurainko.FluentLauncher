using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Network;

public record ResourceUpdateInfo
{
    public bool HasUpdate { get; init; }
    public string CurrentVersion { get; init; } = string.Empty;
    public string LatestVersion { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public ModrinthFile? ModrinthFile { get; init; }
    public CurseForgeFile? CurseForgeFile { get; init; }
}

public record ResourceDependencyInfo
{
    public string ProjectId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public ModrinthFile? ModrinthFile { get; init; }
    public CurseForgeFile? CurseForgeFile { get; init; }
    public bool IsInstalled { get; set; }
}

public class ResourceUpdateService(
    HttpClient httpClient,
    ModrinthClient modrinthClient,
    CurseForgeClient curseForgeClient)
{
    private const string ModrinthApiUrl = "https://api.modrinth.com/v2/";

    /// <summary>
    /// Searches Modrinth or CurseForge to check if an update is available for the given resource.
    /// </summary>
    public async Task<ResourceUpdateInfo?> CheckForUpdateAsync(
        string resourceName,
        string? currentVersion,
        string mcVersion,
        string loader,
        string resourceType = "mod")
    {
        try
        {
            // 1. Try search on Modrinth
            string searchUrl = $"{ModrinthApiUrl}search?query={Uri.EscapeDataString(resourceName)}&limit=5&facets=[[\"project_type:{resourceType}\"]]";
            var searchResponse = await httpClient.GetStringAsync(searchUrl);
            var searchNode = JsonNode.Parse(searchResponse);
            var hits = searchNode?["hits"]?.AsArray();

            if (hits != null && hits.Count > 0)
            {
                // Find matching project
                var match = hits.FirstOrDefault(h =>
                    string.Equals(h?["title"]?.GetValue<string>(), resourceName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(h?["slug"]?.GetValue<string>(), resourceName, StringComparison.OrdinalIgnoreCase))
                    ?? hits[0];

                string? projectId = match?["project_id"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(projectId))
                {
                    string versionsUrl = $"{ModrinthApiUrl}project/{projectId}/version";
                    var versionsResponse = await httpClient.GetStringAsync(versionsUrl);
                    var versionsNode = JsonNode.Parse(versionsResponse)?.AsArray();

                    if (versionsNode != null)
                    {
                        var compatibleVersion = versionsNode.FirstOrDefault(v =>
                        {
                            var gameVersions = v?["game_versions"]?.AsArray().Select(x => x?.GetValue<string>()).Where(x => x != null) ?? [];
                            var loaders = v?["loaders"]?.AsArray().Select(x => x?.GetValue<string>()).Where(x => x != null) ?? [];

                            bool mcMatch = string.IsNullOrEmpty(mcVersion) || gameVersions.Contains(mcVersion);
                            bool loaderMatch = string.IsNullOrEmpty(loader) || loaders.Contains(loader.ToLowerInvariant()) || loaders.Contains("minecraft");

                            return mcMatch && loaderMatch;
                        });

                        if (compatibleVersion != null)
                        {
                            string versionNum = compatibleVersion["version_number"]?.GetValue<string>() ?? compatibleVersion["name"]?.GetValue<string>() ?? "";
                            var primaryFile = compatibleVersion["files"]?.AsArray().FirstOrDefault(f => f?["primary"]?.GetValue<bool>() == true)
                                ?? compatibleVersion["files"]?.AsArray().FirstOrDefault();

                            if (primaryFile != null)
                            {
                                string fileUrl = primaryFile["url"]?.GetValue<string>() ?? "";
                                string fileName = primaryFile["filename"]?.GetValue<string>() ?? "";

                                bool isNewer = !string.IsNullOrEmpty(currentVersion)
                                    ? !string.Equals(currentVersion.Trim(), versionNum.Trim(), StringComparison.OrdinalIgnoreCase)
                                    : true;

                                return new ResourceUpdateInfo
                                {
                                    HasUpdate = isNewer,
                                    CurrentVersion = currentVersion ?? "Unknown",
                                    LatestVersion = versionNum,
                                    FileName = fileName,
                                    DownloadUrl = fileUrl,
                                    Source = "Modrinth",
                                    ModrinthFile = new ModrinthFile
                                    {
                                        FileName = fileName,
                                        Url = fileUrl,
                                        McVersion = mcVersion,
                                        Loaders = [loader.ToLowerInvariant()]
                                    }
                                };
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Finds required dependencies for a mod on Modrinth.
    /// </summary>
    public async Task<List<ResourceDependencyInfo>> GetDependenciesAsync(
        string modName,
        string mcVersion,
        string loader,
        IEnumerable<string> existingModFileNames)
    {
        var result = new List<ResourceDependencyInfo>();

        try
        {
            string searchUrl = $"{ModrinthApiUrl}search?query={Uri.EscapeDataString(modName)}&limit=5&facets=[[\"project_type:mod\"]]";
            var searchResponse = await httpClient.GetStringAsync(searchUrl);
            var searchNode = JsonNode.Parse(searchResponse);
            var hits = searchNode?["hits"]?.AsArray();

            if (hits == null || hits.Count == 0) return result;

            var match = hits.FirstOrDefault(h =>
                string.Equals(h?["title"]?.GetValue<string>(), modName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(h?["slug"]?.GetValue<string>(), modName, StringComparison.OrdinalIgnoreCase))
                ?? hits[0];

            string? projectId = match?["project_id"]?.GetValue<string>();
            if (string.IsNullOrEmpty(projectId)) return result;

            string versionsUrl = $"{ModrinthApiUrl}project/{projectId}/version";
            var versionsResponse = await httpClient.GetStringAsync(versionsUrl);
            var versionsNode = JsonNode.Parse(versionsResponse)?.AsArray();

            if (versionsNode == null) return result;

            var compatibleVersion = versionsNode.FirstOrDefault(v =>
            {
                var gameVersions = v?["game_versions"]?.AsArray().Select(x => x?.GetValue<string>()).Where(x => x != null) ?? [];
                var loaders = v?["loaders"]?.AsArray().Select(x => x?.GetValue<string>()).Where(x => x != null) ?? [];

                bool mcMatch = string.IsNullOrEmpty(mcVersion) || gameVersions.Contains(mcVersion);
                bool loaderMatch = string.IsNullOrEmpty(loader) || loaders.Contains(loader.ToLowerInvariant());

                return mcMatch && loaderMatch;
            }) ?? versionsNode.FirstOrDefault();

            if (compatibleVersion == null) return result;

            var dependencies = compatibleVersion["dependencies"]?.AsArray();
            if (dependencies == null || dependencies.Count == 0) return result;

            var existingNamesList = existingModFileNames.Select(f => f.ToLowerInvariant()).ToList();

            foreach (var dep in dependencies)
            {
                string? depType = dep?["dependency_type"]?.GetValue<string>();
                if (depType != "required") continue;

                string? depProjectId = dep?["project_id"]?.GetValue<string>();
                if (string.IsNullOrEmpty(depProjectId)) continue;

                // Fetch dependency project details
                try
                {
                    string depProjUrl = $"{ModrinthApiUrl}project/{depProjectId}";
                    var projResponse = await httpClient.GetStringAsync(depProjUrl);
                    var projNode = JsonNode.Parse(projResponse);
                    string depTitle = projNode?["title"]?.GetValue<string>() ?? depProjectId;

                    // Fetch latest version for this dependency
                    string depVerUrl = $"{ModrinthApiUrl}project/{depProjectId}/version";
                    var depVerResponse = await httpClient.GetStringAsync(depVerUrl);
                    var depVerNode = JsonNode.Parse(depVerResponse)?.AsArray();

                    var compDepVer = depVerNode?.FirstOrDefault(v =>
                    {
                        var gv = v?["game_versions"]?.AsArray().Select(x => x?.GetValue<string>()).Where(x => x != null) ?? [];
                        var ld = v?["loaders"]?.AsArray().Select(x => x?.GetValue<string>()).Where(x => x != null) ?? [];
                        return (string.IsNullOrEmpty(mcVersion) || gv.Contains(mcVersion)) &&
                               (string.IsNullOrEmpty(loader) || ld.Contains(loader.ToLowerInvariant()));
                    }) ?? depVerNode?.FirstOrDefault();

                    if (compDepVer != null)
                    {
                        var primaryFile = compDepVer["files"]?.AsArray().FirstOrDefault(f => f?["primary"]?.GetValue<bool>() == true)
                            ?? compDepVer["files"]?.AsArray().FirstOrDefault();

                        if (primaryFile != null)
                        {
                            string filename = primaryFile["filename"]?.GetValue<string>() ?? $"{depTitle}.jar";
                            string fileUrl = primaryFile["url"]?.GetValue<string>() ?? "";

                            bool isInstalled = existingNamesList.Any(e =>
                                e.Contains(depTitle.ToLowerInvariant().Replace(" ", "")) ||
                                e.Contains(filename.ToLowerInvariant()));

                            result.Add(new ResourceDependencyInfo
                            {
                                ProjectId = depProjectId,
                                Name = depTitle,
                                FileName = filename,
                                DownloadUrl = fileUrl,
                                IsInstalled = isInstalled,
                                ModrinthFile = new ModrinthFile
                                {
                                    FileName = filename,
                                    Url = fileUrl,
                                    McVersion = mcVersion,
                                    Loaders = [loader.ToLowerInvariant()]
                                }
                            });
                        }
                    }
                }
                catch { }
            }
        }
        catch { }

        return result;
    }
}

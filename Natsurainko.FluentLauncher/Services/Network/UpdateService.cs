#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Downloader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Network;

internal partial class UpdateService
{
    public const string GithubReleasesApi = "https://api.github.com/repos/Xcube-Studio/Natsurainko.FluentLauncher/releases";
    public const string GitHubInstallerReleasesApi = "https://api.github.com/repos/Xcube-Studio/FluentLauncher.PreviewChannel.PackageInstaller/releases/latest";

    private readonly HttpClient _httpClient = new();
    private readonly DownloadService _downloadService;
    private readonly LocalStorageService _localStorageService;

    public UpdateService(DownloadService downloadService, LocalStorageService localStorageService)
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Natsurainko.FluentLauncher", App.Version.GetVersionString()));
        _downloadService = downloadService;
        _localStorageService = localStorageService;
    }

    public async Task<(bool, JsonNode?)> CheckUpdateRelease()
    {
        string releasesContent = await _httpClient.GetStringAsync(GithubReleasesApi);
        string pattern = @"(?<=``` json)([\s\S]+?)(?=```)";

        foreach (var node in JsonArray.Parse(releasesContent)!.AsArray())
        {
            if (node!.AsObject().ContainsKey("prerelease") && node["prerelease"]!.GetValue<bool>())
            {
                string body = node["body"]!.GetValue<string>();
                Match match = Regex.Match(body, pattern);

                if (!match.Success)
                    continue;

                JsonNode jsonBody = JsonNode.Parse(match.Groups[1].Value)!;

                if (jsonBody["build"]!.GetValue<int>() > App.Version.Revision)
                    return (true, node);
            }
        }

        return (false, null);
    }

    public async Task<(bool, string?)> CheckInstallerUpdateRelease()
    {
        string releasesContent = await _httpClient.GetStringAsync(GitHubInstallerReleasesApi);
        string architecture = GetArchitecture();

        var node = JsonNode.Parse(releasesContent);
        if (node!["assets"]!.AsArray().FirstOrDefault(x => x!["name"]!.GetValue<string>() == $"PackageInstaller-{architecture}.exe") is JsonNode asset)
        {
            string downloadUrl = asset["browser_download_url"]!.GetValue<string>();
            FileInfo file = _localStorageService.GetFile($"launcher-update\\PackageInstaller-{architecture}.exe");

            if (file.Exists && file.Length == asset["size"]!.GetValue<long>())
                return (false, null);

            return (true, downloadUrl);
        }

        return (false, null);
    }

    public DownloadTask CreatePackageInstallerDownloadTask(string installerDownloadUrl, string? proxy = null)
    {
        if (!string.IsNullOrEmpty(proxy))
            installerDownloadUrl = proxy + installerDownloadUrl;

        return _downloadService.Downloader.CreateDownloadTask(
            installerDownloadUrl, _localStorageService.GetFile($"launcher-update\\PackageInstaller-{GetArchitecture()}.exe").FullName);
    }

    public DownloadTask CreateUpdatePackageDownloadTask(JsonNode releaseJson, string? proxy = null)
    {
        string architecture = GetArchitecture();
        string downloadUrl = releaseJson["assets"]!.AsArray()
            .FirstOrDefault(x => x!["name"]!.GetValue<string>() == $"updatePackage-{architecture}.zip")!
            ["browser_download_url"]!.GetValue<string>();

        if (!string.IsNullOrEmpty(proxy))
            downloadUrl = proxy + downloadUrl;

        return _downloadService.Downloader.CreateDownloadTask(
            downloadUrl, _localStorageService.GetFile($"launcher-update\\updatePackage-{architecture}.zip").FullName);
    }

    public async Task<(bool, string?)> RunInstaller()
    {
        string architecture = GetArchitecture();
        FileInfo file = _localStorageService.GetFile($"launcher-update\\PackageInstaller-{architecture}.exe");

        if (!file.Exists) throw new FileNotFoundException("PackageInstaller not found");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = file.FullName,
                UseShellExecute = true,
                Verb = "runas",
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                WorkingDirectory = file.DirectoryName
            }
        };

        process.Start();
        //process.BeginErrorReadLine();
        //process.BeginOutputReadLine();

        await process.WaitForExitAsync();
        return (false, ""); // If the installer exited without error, the launcher should be closed during the installation.
    }

    private static string GetArchitecture() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        _ => throw new NotSupportedException("not supported architecture")
    };
}

#endif
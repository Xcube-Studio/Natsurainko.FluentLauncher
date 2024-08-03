using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Management.Downloader;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Network;

internal class CacheInterfaceService
{
    public const string LauncherMetaVersionManifest = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
    public const string LauncherContentPatchNotes = "https://launchercontent.mojang.com/v2/javaPatchNotes.json";
    public const string LauncherContentNews = "https://launchercontent.mojang.com/v2/news.json";
    
    public string VersionManifest => _settingsService.CurrentDownloadSource switch
    {
        "Bmclapi" => DownloadMirrors.Bmclapi.VersionManifestUrl,
        _ => LauncherContentPatchNotes
    };

    private readonly LocalStorageService _localStorageService;
    private readonly SettingsService _settingsService;

    public CacheInterfaceService(SettingsService settingsService, LocalStorageService localStorageService)
    {
        _settingsService = settingsService;
        _localStorageService = localStorageService;
    }

    public Task<string?> RequestStringAsync(string url, InterfaceRequestMethod method, string? targetFileName = default)
        => RequestStringAsync(url, method, task => { }, targetFileName);

    public async Task<string?> RequestStringAsync(string url, InterfaceRequestMethod method, Action<Task<string>> func, string? targetFileName = default)
    {
        var fileInfo = _localStorageService.GetFile(targetFileName ?? GetDefaultFileName(url));

        if (!fileInfo.Directory!.Exists) 
            fileInfo.Directory.Create();

        async Task<string> GetStringFromInterface(bool writeToLocal = false)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            using var responseMessage = await HttpUtils.HttpClient.SendAsync(requestMessage);

            responseMessage.EnsureSuccessStatusCode();
            var content = await responseMessage.Content.ReadAsStringAsync();

            if (writeToLocal) 
                await File.WriteAllTextAsync(fileInfo.FullName, content);

            return content;
        }

        if (method == InterfaceRequestMethod.AlwaysLatest)
           await GetStringFromInterface();

        if (fileInfo.Exists && method == InterfaceRequestMethod.Static)
            return await File.ReadAllTextAsync(fileInfo.FullName);

        if (method == InterfaceRequestMethod.PreferredLocal)
        {
            _ = GetStringFromInterface(writeToLocal: true).ContinueWith(func);

            return fileInfo.Exists 
                ? await File.ReadAllTextAsync(fileInfo.FullName)
                : null;
        }

        return await GetStringFromInterface(writeToLocal: true);
    }

    public Task<Stream?> RequestStreamAsync(string url, InterfaceRequestMethod method, string? targetFileName = default)
        => RequestStreamAsync(url, method, task => { }, targetFileName);

    public async Task<Stream?> RequestStreamAsync(string url, InterfaceRequestMethod method, Action<Task<Stream>> func, string? targetFileName = default)
    {
        var fileInfo = _localStorageService.GetFile(targetFileName ?? GetDefaultFileName(url));

        if (!fileInfo.Directory!.Exists)
            fileInfo.Directory.Create();

        async Task<Stream> GetStreamFromInterface(bool writeToLocal = false)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            using var responseMessage = await HttpUtils.HttpClient.SendAsync(requestMessage);

            responseMessage.EnsureSuccessStatusCode();
            using var contentStream = await responseMessage.Content.ReadAsStreamAsync();

            if (writeToLocal)
            {
                using var fileStream = File.Create(fileInfo.FullName);
                using var rentMemory = HttpUtils.MemoryPool.Rent(1024);
                int readMemory = 0;

                while ((readMemory = await contentStream.ReadAsync(rentMemory.Memory)) > 0)
                    await fileStream.WriteAsync(rentMemory.Memory[..readMemory]);
            }

            return File.OpenRead(fileInfo.FullName);
        }

        if (method == InterfaceRequestMethod.AlwaysLatest)
            await GetStreamFromInterface();

        if (fileInfo.Exists && method == InterfaceRequestMethod.Static)
            return File.OpenRead(fileInfo.FullName);

        if (method == InterfaceRequestMethod.PreferredLocal)
        {
            _ = GetStreamFromInterface(writeToLocal: true).ContinueWith(func);

            return fileInfo.Exists
                ? File.OpenRead(fileInfo.FullName)
                : null;
        }

        return await GetStreamFromInterface(writeToLocal: true);
    }

    private string GetDefaultFileName(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentNullException(nameof(url));

        var guid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(url)));
        return $"cache-unknown-interfaces\\{guid:N}";
    }
}
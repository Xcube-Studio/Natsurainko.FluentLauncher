using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Nrk.FluentCore.Utils.PlayerTextureHelper;

namespace Natsurainko.FluentLauncher.Services.Network;

internal class CacheInterfaceService(
    SettingsService settingsService,
    LocalStorageService localStorageService,
    DownloadService downloadService,
    HttpClient httpClient)
{
    public const string LauncherMetaVersionManifest = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
    public const string LauncherContentPatchNotes = "https://launchercontent.mojang.com/v2/javaPatchNotes.json";
    public const string LauncherContentNews = "https://launchercontent.mojang.com/v2/news.json";

    public string VersionManifest => settingsService.CurrentDownloadSource switch
    {
        "Bmclapi" => "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        _ => LauncherMetaVersionManifest
    };

    public Task<string?> RequestStringAsync(string url, InterfaceRequestMethod method, string? targetFileName = default)
        => RequestStringAsync(url, method, task => { }, targetFileName);

    public async Task<string?> RequestStringAsync(string url, InterfaceRequestMethod method, Action<Task<string>> func, string? targetFileName = default)
    {
        var fileInfo = localStorageService.GetFile(targetFileName ?? GetDefaultFileName(url));

        if (!fileInfo.Directory!.Exists)
            fileInfo.Directory.Create();

        async Task<string> GetStringFromInterface(bool writeToLocal = false)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            using var responseMessage = await httpClient.SendAsync(requestMessage);

            responseMessage.EnsureSuccessStatusCode();
            var content = await responseMessage.Content.ReadAsStringAsync();

            if (writeToLocal)
                await File.WriteAllTextAsync(fileInfo.FullName, content);

            return content;
        }

        if (method == InterfaceRequestMethod.AlwaysLatest)
            return await GetStringFromInterface();

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
        => RequestStreamAsync(url, method, task => task.Result.Close(), targetFileName);

    public async Task<Stream?> RequestStreamAsync(string url, InterfaceRequestMethod method, Action<Task<Stream>> func, string? targetFileName = default)
    {
        var fileInfo = localStorageService.GetFile(targetFileName ?? GetDefaultFileName(url));

        if (!fileInfo.Directory!.Exists)
            fileInfo.Directory.Create();

        async Task<Stream> GetStreamFromInterface(bool writeToLocal = false)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            using var responseMessage = await httpClient.SendAsync(requestMessage);

            responseMessage.EnsureSuccessStatusCode();

            if (writeToLocal)
            {
                await using var contentStream = await responseMessage.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(fileInfo.FullName);

                using var rentMemory = MemoryPool<byte>.Shared.Rent(1024);
                int readMemory = 0;

                while ((readMemory = await contentStream.ReadAsync(rentMemory.Memory)) > 0)
                    await fileStream.WriteAsync(rentMemory.Memory[..readMemory]);

                await fileStream.FlushAsync();

                return File.OpenRead(fileInfo.FullName);
            }

            return await responseMessage.Content.ReadAsStreamAsync();
        }

        if (method == InterfaceRequestMethod.AlwaysLatest)
            return await GetStreamFromInterface();

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

    public async Task<PlayerTextureProfile> RequestTextureProfileAsync(Account account, InterfaceRequestMethod method = InterfaceRequestMethod.Static)
    {
        if (method != InterfaceRequestMethod.Static && method != InterfaceRequestMethod.RefreshStatic)
            throw new InvalidOperationException();

        if (account is OfflineAccount)
        {
            return new PlayerTextureProfile()
            {
                Type = AccountType.Offline,
                Uuid = account.Uuid,
            };
        }

        var profileFile = localStorageService.GetFile($"cache-textures-profiles\\{account.GetProfileServiceIdentifier()}\\{account.Uuid}.json");

        if (profileFile.Exists && method == InterfaceRequestMethod.Static)
            return JsonSerializer.Deserialize(await File.ReadAllTextAsync(profileFile.FullName), FLSerializerContext.Default.PlayerTextureProfile)
                ?? throw new InvalidDataException();

        var textureProfile = await account.GetTextureProfileAsync();
        var profileFileContent = JsonSerializer.Serialize(textureProfile, FLSerializerContext.Default.PlayerTextureProfile);

        profileFile.Directory!.Create();
        await File.WriteAllTextAsync(profileFile.FullName, profileFileContent);

        return textureProfile;
    }

    public async Task CacheTexturesAsync(Account account)
    {
        if (account is OfflineAccount) return;

        var textureProfile = await RequestTextureProfileAsync(account, InterfaceRequestMethod.RefreshStatic);
        string skinTexturePath = textureProfile.GetSkinTexturePath(out bool isDefalutSkin);

        // If the skin is default, we don't need to download it
        if (isDefalutSkin || textureProfile.ActiveSkin?.Url is null) return;

        await downloadService.Downloader.DownloadFileAsync(new(textureProfile.ActiveSkin.Url, skinTexturePath))
            .ContinueWith(t => WeakReferenceMessenger.Default.Send(new SkinTextureUpdatedMessage(account)), TaskContinuationOptions.OnlyOnRanToCompletion);

        if (textureProfile.TryGetCapeTexturePath(out string? capeTexturePath) && textureProfile.ActiveCape?.Url is not null)
        {
            await downloadService.Downloader.DownloadFileAsync(new(textureProfile.ActiveCape.Url, capeTexturePath))
                .ContinueWith(t => WeakReferenceMessenger.Default.Send(new CapeTextureUpdatedMessage(account)), TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }

    private static string GetDefaultFileName(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentNullException(nameof(url));

        var guid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(url)));
        return $"cache-unknown-interfaces\\{guid:N}";
    }
}
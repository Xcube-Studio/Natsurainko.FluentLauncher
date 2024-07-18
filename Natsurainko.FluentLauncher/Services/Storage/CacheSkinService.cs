using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Management.Downloader.Data;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Storage;

internal class CacheSkinService
{
    private readonly LocalStorageService _localStorageService;

    public CacheSkinService(LocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public string GetSkinFilePath(Account account)
    {
        if (account.Type == AccountType.Offline)
            return "ms-appx:///Assets/Icons/steve.png";

        return _localStorageService.GetFile($"cache-skins\\{account.Type}\\{account.Uuid}.png").FullName;
    }

    public async Task<bool> CacheSkinOfAccount(Account account)
    {
        if (account == null || account.Type.Equals(AccountType.Offline))
            return false;

        var authorization = new Tuple<string, string>("Bearer", account.AccessToken);
        var skinUrl = string.Empty;

        if (account is YggdrasilAccount yggdrasil)
        {
            using var responseMessage = HttpUtils.HttpGet(
                yggdrasil.YggdrasilServerUrl +
                "/sessionserver/session/minecraft/profile/" +
                yggdrasil.Uuid.ToString("N").ToLower()
                , authorization);

            responseMessage.EnsureSuccessStatusCode();

            var jsonBase64 = JsonNode.Parse(responseMessage.Content.ReadAsString())!["properties"]![0]!["value"]!;
            var json = JsonNode.Parse(jsonBase64.GetValue<string>().ConvertFromBase64());

            skinUrl = json!["textures"]?["SKIN"]?["url"]?.GetValue<string>();
        }

        if (account is MicrosoftAccount microsoft)
        {
            using var responseMessage = HttpUtils.HttpGet("https://api.minecraftservices.com/minecraft/profile", authorization);
            responseMessage.EnsureSuccessStatusCode();

            var json = JsonNode.Parse(responseMessage.Content.ReadAsString())!["skins"]!
                .AsArray().Where(item => (item!["state"]?.GetValue<string>().Equals("ACTIVE")).GetValueOrDefault()).FirstOrDefault();

            skinUrl = json!["url"]!.GetValue<string>();
        }

        var skinFilePath = GetSkinFilePath(account);
        if (!string.IsNullOrEmpty(skinUrl))
        {
            var downloadResult = await HttpUtils.DownloadElementAsync(new DownloadElement
            {
                AbsolutePath = skinFilePath,
                Url = skinUrl,
            });

            if (downloadResult.IsFaulted)
                return false;
        }
        else File.Copy((await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/skin_steve.png"))).Path, skinFilePath, true);

        WeakReferenceMessenger.Default.Send(new AccountSkinCacheUpdatedMessage(account));

        return true;
    }
}

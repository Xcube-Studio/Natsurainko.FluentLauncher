using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Nrk.FluentCore.Authentication;

using Nrk.FluentCore.Utils;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Network;

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
            skinUrl = await SkinHelper.GetSkinUrlAsync(yggdrasil);

        if (account is MicrosoftAccount microsoft)
            skinUrl = await SkinHelper.GetSkinUrlAsync(microsoft);

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
        else return false;

        WeakReferenceMessenger.Default.Send(new AccountSkinCacheUpdatedMessage(account));

        return true;
    }
}

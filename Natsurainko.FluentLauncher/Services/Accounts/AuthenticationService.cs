using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.DefaultComponents.Authenticate;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AuthenticationService
{
    internal const string ClientId = "0844e754-1d2e-4861-8e2b-18059609badb";

    internal const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";

    private readonly AccountService _accountService;
    private readonly SkinCacheService _skinCacheService;

    public AuthenticationService(AccountService accountService, SkinCacheService skinCacheService)
    {
        _accountService = accountService;
        _skinCacheService = skinCacheService;
    }

    public Task RefreshCurrentAccountAsync() => Task.Run(RefreshCurrentAccount);

    public void RefreshCurrentAccount()
    {
        Account activeAccount = _accountService.ActiveAccount;
        Account refreshedAccount = default;
        if (activeAccount is MicrosoftAccount microsoftAccount)
        {
            refreshedAccount = DefaultMicrosoftAuthenticator
                .CreateForRefresh(ClientId, RedirectUrl, microsoftAccount)
                .Authenticate();
        }
        else if (activeAccount is YggdrasilAccount yggdrasilAccount)
        {
            refreshedAccount = DefaultYggdrasilAuthenticator
                .CreateForRefresh(yggdrasilAccount)
                .Authenticate()
                .First(account => account.Uuid.Equals(activeAccount.Uuid));
        }
        else if (activeAccount is OfflineAccount offlineAccount)
            refreshedAccount = new DefaultOfflineAuthenticator(activeAccount.Name, activeAccount.Uuid).Authenticate();

        App.DispatcherQueue.TryEnqueue(() =>
        {
            _accountService.Remove(activeAccount);

#pragma warning disable CS0612 // Type or member is obsolete
            _accountService.AddAccount(refreshedAccount);
#pragma warning restore CS0612 // Type or member is obsolete
            _accountService.Activate(refreshedAccount);

            Task.Run(() => _skinCacheService.TryCacheSkin(refreshedAccount));
        });
    }

    public void RefreshContainedAccount(Account account)
    {
        Account refreshedAccount = default;

        if (account is MicrosoftAccount microsoftAccount)
        {
            refreshedAccount = DefaultMicrosoftAuthenticator
                .CreateForRefresh(ClientId, RedirectUrl, microsoftAccount)
                .Authenticate();
        }
        else if (account is YggdrasilAccount yggdrasilAccount)
        {
            refreshedAccount = DefaultYggdrasilAuthenticator
                .CreateForRefresh(yggdrasilAccount)
                .Authenticate()
                .First(account => account.Uuid.Equals(account.Uuid));
        }
        else if (account is OfflineAccount offlineAccount)
            refreshedAccount = new DefaultOfflineAuthenticator(account.Name, account.Uuid).Authenticate();

        App.DispatcherQueue.TryEnqueue(() =>
        {
            _accountService.Remove(account);
            _accountService.AddAccount(refreshedAccount);

            Task.Run(() => _skinCacheService.TryCacheSkin(refreshedAccount));
        });
    }

    public OfflineAccount AuthenticateOffline(string name, string uuid)
        => new DefaultOfflineAuthenticator(name, uuid == null ? null : Guid.Parse(uuid)).Authenticate();

    public YggdrasilAccount[] AuthenticateYggdrasil(string url, string email, string password)
        => DefaultYggdrasilAuthenticator.CreateForLogin(email, password, url).Authenticate();

    public MicrosoftAccount AuthenticateMicrosoft(DeviceFlowResponse deviceFlowResponse, Action<string> progressChanged)
    {
        var authenticator = DefaultMicrosoftAuthenticator.CreateFromDeviceFlow(ClientId, RedirectUrl, deviceFlowResponse.OAuth20TokenResponse);
        //authenticator.ProgressChanged += (_, e) => progressChanged(e.Item2); //TODO:

        return authenticator.Authenticate();
    }

    public MicrosoftAccount AuthenticateMicrosoft(string accessCode, Action<string> progressChanged)
    {
        var authenticator = DefaultMicrosoftAuthenticator.CreateForLogin(ClientId, RedirectUrl, accessCode);
        //authenticator.ProgressChanged += (_, e) => progressChanged(e.Item2);

        return (MicrosoftAccount)authenticator.Authenticate();
    }
}

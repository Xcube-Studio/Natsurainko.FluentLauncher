using Nrk.FluentCore.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AuthenticationService
{
    internal const string MicrosoftClientId = "0844e754-1d2e-4861-8e2b-18059609badb";
    internal const string MicrosoftRedirectUrl = "https://login.live.com/oauth20_desktop.srf";

    // Authenticators
    // TODO: Move to config file and remove from source control
    private readonly MicrosoftAuthenticator _microsoftAuthenticator = new(MicrosoftClientId, MicrosoftRedirectUrl);

    private readonly OfflineAuthenticator _offlineAuthenticator = new();

    public AuthenticationService() { }

    public Task<MicrosoftAccount> LoginMicrosoftAsync(string code, IProgress<MicrosoftAuthenticationProgress>? progress = null, CancellationToken cancellationToken = default)
        => _microsoftAuthenticator.LoginAsync(code, progress, cancellationToken);

    public Task<MicrosoftAccount> LoginMicrosoftAsync(OAuth2Tokens msaTokens, IProgress<MicrosoftAuthenticationProgress>? progress = null, CancellationToken cancellationToken = default)
        => _microsoftAuthenticator.LoginAsync(msaTokens, progress, cancellationToken);

    public Task<OAuth2Tokens> AuthMsaFromDeviceFlowAsync(
        Action<OAuth2DeviceCodeResponse> receiveUserCodeAction,
        CancellationToken cancellationToken = default)
        => _microsoftAuthenticator.AuthMsaFromDeviceFlowAsync(receiveUserCodeAction, cancellationToken);

    public Task<YggdrasilAccount[]> LoginYggdrasilAsync(string serverUrl, string email, string password, string serverName, CancellationToken cancellationToken = default)
        => new YggdrasilAuthenticator(serverUrl) { ServerName = serverName }.LoginAsync(email, password, cancellationToken);

    //public async Task<YggdrasilAccount> LoginYggdrasilAsync(string serverUrl, OAuth2TokenResponse oAuth2TokenResponse, CancellationToken cancellationToken = default)
    //{
    //    var authAuthenticator = await YggdrasilOAuthAuthenticator.CreateFromServerPublicClientAsync(serverUrl);
    //    return await authAuthenticator.LoginAsync(oAuth2TokenResponse, cancellationToken);
    //}

    //public async Task<OAuth2TokenResponse> AuthYggdrasilFromDeviceFlowAsync(string serverUrl, Action<OAuth2DeviceCodeResponse> receiveUserCodeAction, CancellationToken cancellationToken = default)
    //{
    //    var authAuthenticator = await YggdrasilOAuthAuthenticator.CreateFromServerPublicClientAsync(serverUrl);
    //    return await authAuthenticator.AuthFromDeviceFlowAsync(receiveUserCodeAction, cancellationToken);
    //}

    public OfflineAccount LoginOffline(string name, string? uuid)
        => _offlineAuthenticator.Login(name, uuid == null ? null : Guid.Parse(uuid));

    public Task<MicrosoftAccount> RefreshAsync(MicrosoftAccount account, CancellationToken cancellationToken = default)
        => _microsoftAuthenticator.RefreshAsync(account, cancellationToken:cancellationToken);

    public Task<YggdrasilAccount> RefreshAsync(YggdrasilAccount account, CancellationToken cancellationToken = default)
        => new YggdrasilAuthenticator(account.YggdrasilServerUrl, account.ClientToken).RefreshAsync(account, cancellationToken);

    //public async Task<YggdrasilAccount> RefreshAsync(YggdrasilAccount account, CancellationToken cancellationToken = default)
    //{
    //    if (!account.MetaData.TryGetValue("authType", out var authType) || authType != "OAuth")
    //    {
    //        return await new YggdrasilAuthenticator(account.YggdrasilServerUrl, account.MetaData["client_token"]).RefreshAsync(account, cancellationToken);
    //    }
    //    else
    //    {
    //        var authAuthenticator = await YggdrasilOAuthAuthenticator.CreateFromServerPublicClientAsync(account.YggdrasilServerUrl);
    //        return await authAuthenticator.RefreshAsync(account, cancellationToken);
    //    }
    //}

    public OfflineAccount Refresh(OfflineAccount account)
        => _offlineAuthenticator.Refresh(account);
}

using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Authentication.Microsoft;
using Nrk.FluentCore.Authentication.Offline;
using Nrk.FluentCore.Authentication.Yggdrasil;
using System;
using System.Linq;
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

    public Task<MicrosoftAccount> LoginMicrosoft(string code, IProgress<MicrosoftAccountAuthenticationProgress>? progress = null)
        => _microsoftAuthenticator.LoginAsync(code, progress);

    public Task<MicrosoftAccount> LoginMicrosoft(
        Action<DeviceCodeResponse> receiveUserCodeAction,
        CancellationToken cancellationToken = default,
        IProgress<MicrosoftAccountAuthenticationProgress>? progress = null
        )
        => _microsoftAuthenticator.LoginFromDeviceFlowAsync(receiveUserCodeAction, cancellationToken, progress);

    public Task<YggdrasilAccount[]> LoginYggdrasil(string serverUrl, string email, string password)
        => new YggdrasilAuthenticator(serverUrl).LoginAsync(email, password);

    public OfflineAccount LoginOffline(string name, string? uuid)
        => _offlineAuthenticator.Login(name, uuid == null ? null : Guid.Parse(uuid));

    public Task<MicrosoftAccount> Refresh(MicrosoftAccount account)
        => _microsoftAuthenticator.RefreshAsync(account);

    public Task<YggdrasilAccount[]> Refresh(YggdrasilAccount account)
        => new YggdrasilAuthenticator(account.YggdrasilServerUrl).RefreshAsync(account);

    public OfflineAccount Refresh(OfflineAccount account)
        => _offlineAuthenticator.Refresh(account);

    // TODO: replace with new APIs
    //public MicrosoftAccount AuthenticateMicrosoft(DeviceFlowResponse deviceFlowResponse, Action<string> progressChanged)
    //{
    //    var authenticator = DefaultMicrosoftAuthenticator.CreateFromDeviceFlow(microsoftClientId, MicrosoftRedirectUrl, deviceFlowResponse.OAuth20TokenResponse);
    //    //authenticator.ProgressChanged += (_, e) => progressChanged(e.Item2); //TODO:

    //    return authenticator.Authenticate();
    //}
}

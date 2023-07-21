using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.DefaultComponets.Authenticate;
using Nrk.FluentCore.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AuthenticationService
{
    internal const string ClientId = "0844e754-1d2e-4861-8e2b-18059609badb";

    internal const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";

    private readonly AccountService _accountService;

    public AuthenticationService(AccountService accountService)
    {
        _accountService = accountService;
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

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _accountService.Remove(activeAccount);

#pragma warning disable CS0612 // Type or member is obsolete
            _accountService.AddAccount(refreshedAccount);
#pragma warning restore CS0612 // Type or member is obsolete
            _accountService.Activate(refreshedAccount);
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

    // TODO: 转为同步方法斌迁移至FluentCore
    public static Task<DeviceFlowResponse> DeviceFlowAuthAsync(Action<DeviceCodeResponse> ReceiveUserCodeAction, out CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        return Task.Run(async () =>
        {
            var deviceAuthPost =
                $"client_id={ClientId}" +
                "&scope=XboxLive.signin%20offline_access";

            using var deviceAuthPostRes = HttpUtils.HttpPost
                ($"https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode", deviceAuthPost, "application/x-www-form-urlencoded");
            
            var deviceAuthResponse = JsonSerializer.Deserialize<DeviceCodeResponse>(await deviceAuthPostRes.Content.ReadAsStringAsync());
            ReceiveUserCodeAction(deviceAuthResponse);

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(deviceAuthResponse.ExpiresIn))
            {
                if (token.IsCancellationRequested)
                    break;

                await Task.Delay(deviceAuthResponse.Interval * 1000);

                var pollingPost =
                    "grant_type=urn:ietf:params:oauth:grant-type:device_code" +
                    $"&client_id={ClientId}" +
                    $"&device_code={deviceAuthResponse.DeviceCode}";

                using var pollingPostRes = HttpUtils.HttpPost
                    ($"https://login.microsoftonline.com/consumers/oauth2/v2.0/token", pollingPost, "application/x-www-form-urlencoded");
                var pollingPostJson = JsonNode.Parse(await pollingPostRes.Content.ReadAsStringAsync());

                if (pollingPostRes.IsSuccessStatusCode)
                    return new()
                    {
                        Success = true,
                        OAuth20TokenResponse = pollingPostJson.Deserialize<OAuth20TokenResponseModel>()
                    };
                else
                {
                    var error = (string)pollingPostJson["error"];
                    if (error.Equals("authorization_declined") ||
                        error.Equals("bad_verification_code") ||
                        error.Equals("expired_token"))
                        break;
                }
            }

            stopwatch.Stop();

            return new DeviceFlowResponse()
            {
                Success = false
            };

        }, token);
    }
}

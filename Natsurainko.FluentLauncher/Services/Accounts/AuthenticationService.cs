using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Natsurainko.Toolkits.Values;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;
using Natsurainko.FluentCore.Module.Authenticator;

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

    public async Task RefreshCurrentAccountAsync()
    {

    }

    public OfflineAccount AuthenticateOffline(string name, string uuid) => new()
    {
        AccessToken = Guid.NewGuid().ToString("N"),
        ClientToken = Guid.NewGuid().ToString("N"),
        Name = name,
        Uuid = string.IsNullOrEmpty(uuid)
            ? GuidHelper.FromString(name)
            : Guid.Parse(uuid)
    };

    public IEnumerable<YggdrasilAccount> AuthenticateYggdrasil(string url, string email, string password)
    {
        var resTask = HttpWrapper.HttpPostAsync(url + "/authserver/authenticate", new LoginRequestModel
        {
            ClientToken = Guid.NewGuid().ToString("N"),
            UserName = email,
            Password = password
        }.ToJson());

        resTask.Wait();
        using var res = resTask.Result;

        var resultTask = res.Content.ReadAsStringAsync();

        resultTask.Wait();
        string result = resultTask.Result;

        res.EnsureSuccessStatusCode();

        var model = JsonConvert.DeserializeObject<YggdrasilResponseModel>(result);

        return model.AvailableProfiles.Select(x => new YggdrasilAccount()
        {
            AccessToken = model.AccessToken,
            ClientToken = model.ClientToken,
            Name = x.Name,
            Uuid = Guid.Parse(x.Id),
            YggdrasilServerUrl = url
        });
    }

    public MicrosoftAccount AuthenticateMicrosoft(DeviceFlowAuthResult deviceFlowAuthResult, Action<string> progressChanged)
    {
        var authenticator = new MicrosoftAuthenticator(deviceFlowAuthResult.OAuth20TokenResponse, ClientId, RedirectUrl);
        authenticator.ProgressChanged += (_, e) => progressChanged(e.Item2);

        return (MicrosoftAccount)authenticator.Authenticate();
    }

    public MicrosoftAccount AuthenticateMicrosoft(string accessCode, Action<string> progressChanged)
    {
        var authenticator = new MicrosoftAuthenticator(accessCode, ClientId, RedirectUrl);
        authenticator.ProgressChanged += (_, e) => progressChanged(e.Item2);

        return (MicrosoftAccount)authenticator.Authenticate();
    }

    public static Task<DeviceFlowAuthResult> DeviceFlowAuthAsync(Action<DeviceAuthorizationResponse> ReceiveUserCodeAction, out CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        return Task.Run(async () =>
        {
            var deviceAuthPost =
                $"client_id={ClientId}" +
                "&scope=XboxLive.signin%20offline_access";

            using var deviceAuthPostRes = await HttpWrapper.HttpPostAsync
                ($"https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode", deviceAuthPost, "application/x-www-form-urlencoded");
            var deviceAuthResponse = JsonConvert.DeserializeObject<DeviceAuthorizationResponse>(await deviceAuthPostRes.Content.ReadAsStringAsync());
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

                using var pollingPostRes = await HttpWrapper.HttpPostAsync
                    ($"https://login.microsoftonline.com/consumers/oauth2/v2.0/token", pollingPost, "application/x-www-form-urlencoded");
                var pollingPostJson = JObject.Parse(await pollingPostRes.Content.ReadAsStringAsync());

                if (pollingPostRes.IsSuccessStatusCode)
                    return new()
                    {
                        Success = true,
                        OAuth20TokenResponse = pollingPostJson.ToObject<OAuth20TokenResponseModel>()
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

            return new DeviceFlowAuthResult()
            {
                Success = false
            };

        }, token);
    }

}

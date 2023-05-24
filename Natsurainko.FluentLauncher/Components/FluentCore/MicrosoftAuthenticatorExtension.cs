using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

internal static class MicrosoftAuthenticatorExtension
{
    public static Task<DeviceFlowAuthResult> DeviceFlowAuthAsync
        (string client_id, Action<DeviceAuthorizationResponse> ReceiveUserCodeAction, out CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        return Task.Run(async () =>
        {
            try
            {
                var deviceAuthPost =
                    $"client_id={client_id}" +
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
                        $"&client_id={client_id}" +
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
            }
            catch (Exception ex) 
            {
                if (ex is OperationCanceledException)
                { 
                }
            }

            return new DeviceFlowAuthResult()
            {
                Success = false
            };

        }, token);
    }
}

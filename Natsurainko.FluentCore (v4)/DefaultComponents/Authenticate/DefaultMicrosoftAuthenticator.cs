using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Events;
using Nrk.FluentCore.Components.Authenticate;
using Nrk.FluentCore.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AuthException = Nrk.FluentCore.Classes.Exceptions.MicrosoftAuthenticateException;
using AuthExceptionType = Nrk.FluentCore.Classes.Enums.MicrosoftAuthenticateExceptionType;
using AuthStep = Nrk.FluentCore.Classes.Enums.MicrosoftAuthenticateStep;

namespace Nrk.FluentCore.DefaultComponents.Authenticate;

public class DefaultMicrosoftAuthenticator : BaseAuthenticator<MicrosoftAccount>
{
    private string _clientId;
    private string _redirectUri;
    private string _code;
    private string _parameterName;
    private OAuth20TokenResponseModel _oAuth20TokenResponse;
    private bool _createdFromDeviceFlow = false;

    public event EventHandler<MicrosoftAuthenticateProgressChangedEventArgs> ProgressChanged;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AuthException"></exception>
    public override MicrosoftAccount Authenticate()
    {
        #region Get Authorization Token

        if (!_createdFromDeviceFlow)
        {
            ProgressChanged?.Invoke(this, (AuthStep.Get_Authorization_Token, 0.2));

            string authCodePost =
                $"client_id={_clientId}" +
                $"&{_parameterName}={_code}" +
                $"&grant_type={(_parameterName.Equals("code") ? "authorization_code" : "refresh_token")}" +
                $"&redirect_uri={_redirectUri}";

            var authCodePostRes = HttpUtils.HttpPost(
                $"https://login.live.com/oauth20_token.srf",
                authCodePost,
                "application/x-www-form-urlencoded");

            authCodePostRes.EnsureSuccessStatusCode();

            _oAuth20TokenResponse = JsonSerializer.Deserialize<OAuth20TokenResponseModel>(authCodePostRes.Content.ReadAsString());
        }

        #endregion

        #region Authenticate with XBL

        ProgressChanged?.Invoke(this, (AuthStep.Authenticate_with_XboxLive, 0.40));

        var xBLReqModel = new XBLAuthenticateRequestModel();
        xBLReqModel.Properties.RpsTicket = xBLReqModel.Properties.RpsTicket.Replace("<access token>", _oAuth20TokenResponse.AccessToken);

        using var xBLReqModelPostRes = HttpUtils.HttpPost(
            $"https://user.auth.xboxlive.com/user/authenticate",
            JsonSerializer.Serialize(xBLReqModel));

        xBLReqModelPostRes.EnsureSuccessStatusCode();

        var xBLResModel = JsonSerializer.Deserialize<XBLAuthenticateResponseModel>(xBLReqModelPostRes.Content.ReadAsString());

        #endregion

        #region Authenticate with XSTS

        ProgressChanged?.Invoke(this, (AuthStep.Obtain_XSTS_token_for_Minecraft, 0.55));

        var xSTSReqModel = new XSTSAuthenticateRequestModel();
        xSTSReqModel.Properties.UserTokens.Add(xBLResModel.Token);

        using var xSTSReqModelPostRes = HttpUtils.HttpPost(
            $"https://xsts.auth.xboxlive.com/xsts/authorize",
            JsonSerializer.Serialize(xSTSReqModel));

        if (xSTSReqModelPostRes.StatusCode.Equals(HttpStatusCode.Unauthorized))
        {
            var error = xSTSReqModelPostRes.Content.ReadAsString();
            var xSTSAuthenticateErrorModel = JsonSerializer.Deserialize<XSTSAuthenticateErrorModel>(error);

            var message = "An error occurred while verifying with Xbox Live";
            if (!string.IsNullOrEmpty(xSTSAuthenticateErrorModel.Message))
                message += $" ({xSTSAuthenticateErrorModel.Message})";

            throw new AuthException(message)
            {
                HelpLink = xSTSAuthenticateErrorModel.XErr switch
                {
                    2148916233 => "The account doesn't have an Xbox account. Once they sign up for one (or login through minecraft.net to create one) " +
                        "then they can proceed with the login. This shouldn't happen with accounts that have purchased Minecraft with a Microsoft account, " +
                        "as they would've already gone through that Xbox signup process.",
                    2148916235 => "The account is from a country where Xbox Live is not available/banned",
                    2148916236 => "The account needs adult verification on Xbox page. (South Korea)",
                    2148916237 => "The account needs adult verification on Xbox page. (South Korea)",
                    2148916238 => "The account is a child (under 18) and cannot proceed unless the account is added to a Family by an adult. " +
                        "This only seems to occur when using a custom Microsoft Azure application. When using the Minecraft launchers client id, " +
                        "this doesn't trigger.",
                    _ => string.Empty
                },
                Step = AuthStep.Obtain_XSTS_token_for_Minecraft,
                Type = AuthExceptionType.XboxLiveError
            };
        };

        xSTSReqModelPostRes.EnsureSuccessStatusCode();

        var xSTSResModel = JsonSerializer.Deserialize<XSTSAuthenticateResponseModel>(xSTSReqModelPostRes.Content.ReadAsString());

        #endregion

        #region Authenticate with Minecraft

        ProgressChanged?.Invoke(this, (AuthStep.Authenticate_with_Minecraft, 0.75));

        string authenticateMinecraftPost =
            $"{{\"identityToken\":\"XBL3.0 x={xBLResModel.DisplayClaims.Xui[0]["uhs"].GetValue<string>()};{xSTSResModel.Token}\"}}";

        using var authenticateMinecraftPostRes = HttpUtils.HttpPost(
            $"https://api.minecraftservices.com/authentication/login_with_xbox",
            authenticateMinecraftPost);

        authenticateMinecraftPostRes.EnsureSuccessStatusCode();

        string access_token = JsonNode.Parse(authenticateMinecraftPostRes.Content.ReadAsString())["access_token"].GetValue<string>();

        #endregion

        var authorization = new Tuple<string, string>("Bearer", access_token);

        #region Checking Game Ownership

        ProgressChanged?.Invoke(this, (AuthStep.Checking_Game_Ownership, 0.80));

        using var checkingGameOwnershipGetRes = HttpUtils.HttpGet(
            $"https://api.minecraftservices.com/entitlements/mcstore",
            authorization);

        checkingGameOwnershipGetRes.EnsureSuccessStatusCode();

        var gameOwnershipItems = JsonNode.Parse(checkingGameOwnershipGetRes.Content.ReadAsString())["items"].AsArray();

        if (!gameOwnershipItems.Any())
            throw new AuthException("An error occurred while checking game ownership")
            {
                HelpLink = "The account doesn't own the game",
                Step = AuthStep.Checking_Game_Ownership,
                Type = AuthExceptionType.GameOwnershipError
            };

        #endregion

        #region Get the profile

        ProgressChanged?.Invoke(this, (AuthStep.Get_the_profile, 0.9));

        using var profileRes = HttpUtils.HttpGet("https://api.minecraftservices.com/minecraft/profile", authorization);

        profileRes.EnsureSuccessStatusCode();

        var microsoftAuthenticationResponse = JsonSerializer.Deserialize<MicrosoftAuthenticationResponse>(profileRes.Content.ReadAsString());

        ProgressChanged?.Invoke(this, (AuthStep.Finished, 1.0));

        return new MicrosoftAccount
        {
            AccessToken = access_token,
            Name = microsoftAuthenticationResponse.Name,
            Uuid = Guid.Parse(microsoftAuthenticationResponse.Id),
            RefreshToken = _oAuth20TokenResponse.RefreshToken,
            LastRefreshTime = DateTime.Now
        };

        #endregion
    }

    public static DefaultMicrosoftAuthenticator CreateForLogin(string clientId, string redirectUri, string code) => new()
    {
        _redirectUri = redirectUri,
        _code = code,
        _clientId = clientId,
        _parameterName = "code"
    };

    public static DefaultMicrosoftAuthenticator CreateForRefresh(string clientId, string redirectUri, MicrosoftAccount account) => new()
    {
        _redirectUri = redirectUri,
        _code = account.RefreshToken,
        _clientId = clientId,
        _parameterName = "refresh_token"
    };

    public static DefaultMicrosoftAuthenticator CreateFromDeviceFlow(string clientId, string redirectUri, OAuth20TokenResponseModel oAuth20TokenResponseModel) => new()
    {
        _redirectUri = redirectUri,
        _clientId = clientId,
        _oAuth20TokenResponse = oAuth20TokenResponseModel,
        _createdFromDeviceFlow = true
    };

    public static Task<DeviceFlowResponse> DeviceFlowAuthAsync(string clientId, Action<DeviceCodeResponse> ReceiveUserCodeAction, out CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        return Task.Run(async () =>
        {
            var deviceAuthPost =
                $"client_id={clientId}" +
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
                    $"&client_id={clientId}" +
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

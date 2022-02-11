using FluentCore.Interface;
using FluentCore.Model.Auth;
using FluentCore.Model.Auth.Microsoft;
using FluentCore.Service.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace FluentCore.Service.Component.Authenticator
{
    public class MicrosoftAuthenticator : IAuthenticator
    {
        public string ClientId { get; set; }

        public string RedirectUri { get; set; }

        public string Code { get; set; }

        public event EventHandler<string> SingleStepBeginning;

        public MicrosoftAuthenticator(string code, string clientId, string redirectUri)
        {
            this.Code = code;
            this.ClientId = clientId;
            this.RedirectUri = redirectUri;
        }

        public Tuple<BaseResponseModel, AuthResponseType> Authenticate() => AuthenticateAsync().GetAwaiter().GetResult();

        public async Task<Tuple<BaseResponseModel, AuthResponseType>> AuthenticateAsync()
        {
            try
            {
                #region Get Authorization Token

                SingleStepBeginning?.Invoke(this, "Getting Authorization Token");

                string authCodePost =
                    $"client_id={this.ClientId}" +
                    $"&code={this.Code}" +
                    "&grant_type=authorization_code" +
                    $"&redirect_uri={this.RedirectUri}";

                var authCodePostRes = await HttpHelper.HttpPostAsync($"https://login.live.com/oauth20_token.srf", authCodePost, "application/x-www-form-urlencoded");
                var oAuth20TokenResponse = JsonConvert.DeserializeObject<OAuth20TokenResponseModel>(await authCodePostRes.Content.ReadAsStringAsync());

                #endregion

                #region Authenticate with XBL

                SingleStepBeginning?.Invoke(this, "Authenticating with XBL");

                var xBLReqModel = new XBLAuthenticateRequestModel();
                xBLReqModel.Properties.RpsTicket = xBLReqModel.Properties.RpsTicket.Replace("<access token>", oAuth20TokenResponse.AccessToken);

                using var xBLReqModelPostRes = await HttpHelper.HttpPostAsync($"https://user.auth.xboxlive.com/user/authenticate", JsonConvert.SerializeObject(xBLReqModel));
                var xBLResModel = JsonConvert.DeserializeObject<XBLAuthenticateResponseModel>(await xBLReqModelPostRes.Content.ReadAsStringAsync());

                #endregion

                #region Authenticate with XSTS

                SingleStepBeginning?.Invoke(this, "Authenticating with XSTS");

                var xSTSReqModel = new XSTSAuthenticateRequestModel();
                xSTSReqModel.Properties.UserTokens.Add(xBLResModel.Token);

                using var xSTSReqModelPostRes = await HttpHelper.HttpPostAsync($"https://xsts.auth.xboxlive.com/xsts/authorize", JsonConvert.SerializeObject(xSTSReqModel));
                var xSTSResModel = JsonConvert.DeserializeObject<XSTSAuthenticateResponseModel>(await xSTSReqModelPostRes.Content.ReadAsStringAsync());

                #endregion

                #region Authenticate with Minecraft

                SingleStepBeginning?.Invoke(this, "Authenticating with Minecraft");

                string authenticateMinecraftPost = "{\"identityToken\":\"XBL3.0 x=<userhash>;<xsts_token>\"}"
                    .Replace("<userhash>", xBLResModel.DisplayClaims.Xui[0]["uhs"].ToString())
                    .Replace("<xsts_token>", xSTSResModel.Token);
                using var authenticateMinecraftPostRes = await HttpHelper.HttpPostAsync($"https://api.minecraftservices.com/authentication/login_with_xbox", authenticateMinecraftPost);
                string access_token = (string)JObject.Parse(await authenticateMinecraftPostRes.Content.ReadAsStringAsync())["access_token"];

                #endregion

                #region Get the profile

                SingleStepBeginning?.Invoke(this, "Getting the profile");

                var authorization = new Tuple<string, string>("Bearer", access_token);
                using var profileRes = await HttpHelper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", authorization);
                var microsoftAuthenticationResponse = JsonConvert.DeserializeObject<MicrosoftAuthenticationResponse>(await profileRes.Content.ReadAsStringAsync());

                microsoftAuthenticationResponse.AccessToken = access_token;
                microsoftAuthenticationResponse.RefreshToken = oAuth20TokenResponse.RefreshToken;
                microsoftAuthenticationResponse.ExpiresIn = oAuth20TokenResponse.ExpiresIn;
                microsoftAuthenticationResponse.Time = DateTime.Now.ToString();

                SingleStepBeginning?.Invoke(this, "Finished");

                #endregion

                return new Tuple<BaseResponseModel, AuthResponseType>(microsoftAuthenticationResponse, AuthResponseType.Succeeded);
            }
            catch
            {
                return new Tuple<BaseResponseModel, AuthResponseType>(null, AuthResponseType.Failed);
            }
        }

        public Tuple<BaseResponseModel, AuthResponseType> Refresh(string refreshToken) => RefreshAsync(refreshToken).GetAwaiter().GetResult();

        public async Task<Tuple<BaseResponseModel, AuthResponseType>> RefreshAsync(string refreshToken)
        {
            try
            {
                #region Get Authorization Token

                SingleStepBeginning?.Invoke(this, "Getting Authorization Token");

                string authCodePost =
                    $"client_id={this.ClientId}" +
                    $"&refresh_token={refreshToken}" +
                    "&grant_type=refresh_token" +
                    $"&redirect_uri={this.RedirectUri}";

                var authCodePostRes = await HttpHelper.HttpPostAsync($"https://login.live.com/oauth20_token.srf", authCodePost, "application/x-www-form-urlencoded");
                var oAuth20TokenResponse = JsonConvert.DeserializeObject<OAuth20TokenResponseModel>(await authCodePostRes.Content.ReadAsStringAsync());

                #endregion

                #region Authenticate with XBL

                SingleStepBeginning?.Invoke(this, "Authenticating with XBL");

                var xBLReqModel = new XBLAuthenticateRequestModel();
                xBLReqModel.Properties.RpsTicket = xBLReqModel.Properties.RpsTicket.Replace("<access token>", oAuth20TokenResponse.AccessToken);

                using var xBLReqModelPostRes = await HttpHelper.HttpPostAsync($"https://user.auth.xboxlive.com/user/authenticate", JsonConvert.SerializeObject(xBLReqModel));
                var xBLResModel = JsonConvert.DeserializeObject<XBLAuthenticateResponseModel>(await xBLReqModelPostRes.Content.ReadAsStringAsync());

                #endregion

                #region Authenticate with XSTS

                SingleStepBeginning?.Invoke(this, "Authenticating with XSTS");

                var xSTSReqModel = new XSTSAuthenticateRequestModel();
                xSTSReqModel.Properties.UserTokens.Add(xBLResModel.Token);

                using var xSTSReqModelPostRes = await HttpHelper.HttpPostAsync($"https://xsts.auth.xboxlive.com/xsts/authorize", JsonConvert.SerializeObject(xSTSReqModel));
                var xSTSResModel = JsonConvert.DeserializeObject<XSTSAuthenticateResponseModel>(await xSTSReqModelPostRes.Content.ReadAsStringAsync());

                #endregion

                #region Authenticate with Minecraft

                SingleStepBeginning?.Invoke(this, "Authenticating with Minecraft");

                string authenticateMinecraftPost = "{\"identityToken\":\"XBL3.0 x=<userhash>;<xsts_token>\"}"
                    .Replace("<userhash>", xBLResModel.DisplayClaims.Xui[0]["uhs"].ToString())
                    .Replace("<xsts_token>", xSTSResModel.Token);
                using var authenticateMinecraftPostRes = await HttpHelper.HttpPostAsync($"https://api.minecraftservices.com/authentication/login_with_xbox", authenticateMinecraftPost);
                string access_token = (string)JObject.Parse(await authenticateMinecraftPostRes.Content.ReadAsStringAsync())["access_token"];

                #endregion

                #region Get the profile

                SingleStepBeginning?.Invoke(this, "Getting the profile");

                var authorization = new Tuple<string, string>("Bearer", access_token);
                using var profileRes = await HttpHelper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", authorization);
                var microsoftAuthenticationResponse = JsonConvert.DeserializeObject<MicrosoftAuthenticationResponse>(await profileRes.Content.ReadAsStringAsync());

                microsoftAuthenticationResponse.AccessToken = access_token;
                microsoftAuthenticationResponse.RefreshToken = oAuth20TokenResponse.RefreshToken;
                microsoftAuthenticationResponse.ExpiresIn = oAuth20TokenResponse.ExpiresIn;
                microsoftAuthenticationResponse.Time = DateTime.Now.ToString();

                SingleStepBeginning?.Invoke(this, "Finished");

                #endregion

                return new Tuple<BaseResponseModel, AuthResponseType>(microsoftAuthenticationResponse, AuthResponseType.Succeeded);
            }
            catch
            {
                return new Tuple<BaseResponseModel, AuthResponseType>(null, AuthResponseType.Failed);
            }
        }

        public void Dispose()
        {

        }
    }
}

using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentCore.Class.Model.Auth.Microsoft;
using Natsurainko.FluentCore.Interface;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Authenticator;

public class MicrosoftAuthenticator : IAuthenticator
{
    public string ClientId { get; set; } = "00000000402b5328";

    public string RedirectUri { get; set; } = "https://login.live.com/oauth20_desktop.srf";

    public string Code { get; set; }

    public MicrosoftAuthenticatorMethod Method { get; set; } = MicrosoftAuthenticatorMethod.Login;

    public event EventHandler<(float, string)> ProgressChanged;

    public MicrosoftAuthenticator() { }

    public MicrosoftAuthenticator(string code) => this.Code = code;

    public MicrosoftAuthenticator(string clientId, string redirectUri)
    {
        this.ClientId = clientId;
        this.RedirectUri = redirectUri;
    }

    public MicrosoftAuthenticator(string code, string clientId, string redirectUri)
    {
        this.Code = code;
        this.ClientId = clientId;
        this.RedirectUri = redirectUri;
    }

    public Account Authenticate()
        => this.AuthenticateAsync().GetAwaiter().GetResult();

    public async Task<Account> AuthenticateAsync()
    {
        #region Get Authorization Token

        ProgressChanged?.Invoke(this, (0.20f, "Getting Authorization Token"));

        string authCodePost =
            $"client_id={this.ClientId}" +
            $"&code={this.Code}" +
            $"&grant_type={(this.Method == MicrosoftAuthenticatorMethod.Login ? "authorization_code" : "refresh_token")}" +
            $"&redirect_uri={this.RedirectUri}";

        var authCodePostRes = await HttpWrapper.HttpPostAsync($"https://login.live.com/oauth20_token.srf", authCodePost, "application/x-www-form-urlencoded");
        var oAuth20TokenResponse = JsonConvert.DeserializeObject<OAuth20TokenResponseModel>(await authCodePostRes.Content.ReadAsStringAsync());

        #endregion

        #region Authenticate with XBL

        ProgressChanged?.Invoke(this, (0.40f, "Authenticating with XBL"));

        var xBLReqModel = new XBLAuthenticateRequestModel();
        xBLReqModel.Properties.RpsTicket = xBLReqModel.Properties.RpsTicket.Replace("<access token>", oAuth20TokenResponse.AccessToken);

        using var xBLReqModelPostRes = await HttpWrapper.HttpPostAsync($"https://user.auth.xboxlive.com/user/authenticate", xBLReqModel.ToJson());
        var xBLResModel = JsonConvert.DeserializeObject<XBLAuthenticateResponseModel>(await xBLReqModelPostRes.Content.ReadAsStringAsync());

        #endregion

        #region Authenticate with XSTS

        ProgressChanged?.Invoke(this, (0.55f, "Authenticating with XSTS"));

        var xSTSReqModel = new XSTSAuthenticateRequestModel();
        xSTSReqModel.Properties.UserTokens.Add(xBLResModel.Token);

        using var xSTSReqModelPostRes = await HttpWrapper.HttpPostAsync($"https://xsts.auth.xboxlive.com/xsts/authorize", xSTSReqModel.ToJson());
        var xSTSResModel = JsonConvert.DeserializeObject<XSTSAuthenticateResponseModel>(await xSTSReqModelPostRes.Content.ReadAsStringAsync());

        #endregion

        #region Authenticate with Minecraft

        ProgressChanged?.Invoke(this, (0.75f, "Authenticating with Minecraft"));

        string authenticateMinecraftPost =
            $"{{\"identityToken\":\"XBL3.0 x={xBLResModel.DisplayClaims.Xui[0]["uhs"]};{xSTSResModel.Token}\"}}";

        using var authenticateMinecraftPostRes = await HttpWrapper.HttpPostAsync($"https://api.minecraftservices.com/authentication/login_with_xbox", authenticateMinecraftPost);
        string access_token = (string)JObject.Parse(await authenticateMinecraftPostRes.Content.ReadAsStringAsync())["access_token"];

        #endregion

        #region Get the profile

        ProgressChanged?.Invoke(this, (0.9f, "Getting the profile"));

        var authorization = new Tuple<string, string>("Bearer", access_token);
        using var profileRes = await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", authorization);
        var microsoftAuthenticationResponse = JsonConvert.DeserializeObject<MicrosoftAuthenticationResponse>(await profileRes.Content.ReadAsStringAsync());

        ProgressChanged?.Invoke(this, (1.0f, "Finished"));

        return new MicrosoftAccount
        {
            AccessToken = access_token,
            Type = AccountType.Microsoft,
            ClientToken = Guid.NewGuid().ToString("N"),
            Name = microsoftAuthenticationResponse.Name,
            Uuid = Guid.Parse(microsoftAuthenticationResponse.Id),
            RefreshToken = oAuth20TokenResponse.RefreshToken,
            DateTime = DateTime.Now
        };

        #endregion
    }
}

using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentCore.Class.Model.Auth.Yggdrasil;
using Natsurainko.FluentCore.Interface;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Authenticator;

public class YggdrasilAuthenticator : IAuthenticator
{
    public string YggdrasilServerUrl { get; private set; }

    public string Email { get; private set; }

    public string Password { get; private set; }

    public string AccessToken { get; private set; }

    public string ClientToken { get; private set; } = Guid.NewGuid().ToString("N");

    public YggdrasilAuthenticatorMethod Method { get; private set; }

    public YggdrasilAuthenticator(string yggdrasilServerUrl = "https://authserver.mojang.com", YggdrasilAuthenticatorMethod method = YggdrasilAuthenticatorMethod.Login)
    {
        this.YggdrasilServerUrl = yggdrasilServerUrl;
        this.Method = method;
    }

    public YggdrasilAuthenticator(YggdrasilAuthenticatorMethod method, string accessToken = default, string clientToken = default, string email = default, string password = default, string yggdrasilServerUrl = "https://authserver.mojang.com")
    {
        this.Email = email;
        this.Password = password;
        this.AccessToken = accessToken;
        this.ClientToken = clientToken;

        this.YggdrasilServerUrl = yggdrasilServerUrl;
        this.Method = method;
    }

    public Account Authenticate()
        => this.AuthenticateAsync().GetAwaiter().GetResult();

    public async Task<Account> AuthenticateAsync()
    {
        string url = this.YggdrasilServerUrl;
        string content = string.Empty;

        switch (this.Method)
        {
            case YggdrasilAuthenticatorMethod.Login:
                url += "/authenticate";
                content = new LoginRequestModel
                {
                    ClientToken = this.ClientToken,
                    UserName = this.Email,
                    Password = this.Password
                }.ToJson();
                break;
            case YggdrasilAuthenticatorMethod.Refresh:
                url += "/refresh";
                content = new
                {
                    clientToken = this.ClientToken,
                    accessToken = this.AccessToken,
                    requestUser = true
                }.ToJson();
                break;
            default:
                break;
        }

        using var res = await HttpWrapper.HttpPostAsync(url, content);
        string result = await res.Content.ReadAsStringAsync();

        res.EnsureSuccessStatusCode();

        var model = JsonConvert.DeserializeObject<YggdrasilResponseModel>(result);
        return new YggdrasilAccount()
        {
            AccessToken = model.AccessToken,
            ClientToken = model.ClientToken,
            Name = model.SelectedProfile.Name,
            Uuid = Guid.Parse(model.SelectedProfile.Id),
            YggdrasilServerUrl = this.YggdrasilServerUrl
        };
    }

    public async Task<bool> ValidateAsync(string accessToken)
    {
        string content = JsonConvert.SerializeObject(
            new YggdrasilRequestModel
            {
                ClientToken = this.ClientToken,
                AccessToken = accessToken
            }
        );

        using var res = await HttpWrapper.HttpPostAsync($"{YggdrasilServerUrl}/validate", content);

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> SignoutAsync()
    {
        string content = JsonConvert.SerializeObject(
            new
            {
                username = this.Email,
                password = this.Password
            }
        );

        using var res = await HttpWrapper.HttpPostAsync($"{YggdrasilServerUrl}/signout", content);

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> InvalidateAsync(string accessToken)
    {
        string content = JsonConvert.SerializeObject(
            new YggdrasilRequestModel
            {
                ClientToken = this.ClientToken,
                AccessToken = accessToken
            }
        );

        using var res = await HttpWrapper.HttpPostAsync($"{YggdrasilServerUrl}/invalidate", content);

        return res.IsSuccessStatusCode;
    }
}

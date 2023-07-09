using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PInvoke.User32;
using System.Xml.Linq;
using Natsurainko.Toolkits.Values;
using Microsoft.WindowsAppSDK.Runtime.Packages;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using Natsurainko.Toolkits.Text;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AuthenticationService
{
    private const string ClientId = "0844e754-1d2e-4861-8e2b-18059609badb";

    private const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";

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
}

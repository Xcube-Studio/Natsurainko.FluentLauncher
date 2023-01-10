using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentCore.Interface;
using Natsurainko.Toolkits.Values;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Authenticator;

public class OfflineAuthenticator : IAuthenticator
{
    public string Name { get; set; }

    public Guid Uuid { get; set; }

    public OfflineAuthenticator(string name, Guid uuid = default)
    {
        this.Name = name;
        this.Uuid = uuid;

        if (this.Uuid == default)
            this.Uuid = GuidHelper.FromString(this.Name);
    }

    public Account Authenticate()
        => new OfflineAccount
        {
            AccessToken = Guid.NewGuid().ToString("N"),
            ClientToken = Guid.NewGuid().ToString("N"),
            Name = this.Name,
            Uuid = this.Uuid
        };

    public Task<Account> AuthenticateAsync()
        => Task.Run(Authenticate);
}

using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Components.Authenticate;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Nrk.FluentCore.DefaultComponents.Authenticate;

public class DefaultOfflineAuthenticator : BaseAuthenticator<OfflineAccount>
{
    private readonly string _name;
    private readonly Guid _uuid;

    public DefaultOfflineAuthenticator(string name, Guid? uuid = null)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _uuid = uuid ?? new Guid(MD5.HashData(Encoding.UTF8.GetBytes(name)));
    }

    public override OfflineAccount Authenticate() => new()
    {
        AccessToken = Guid.NewGuid().ToString("N"),
        Name = _name,
        Uuid = _uuid
    };
}

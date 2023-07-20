using Nrk.FluentCore.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Datas.Authenticate;

/// <summary>
/// 表示一个账户
/// </summary>
public abstract record Account
{
    /// <summary>
    /// 账户类型
    /// </summary>
    public virtual AccountType Type { get; init; }

    /// <summary>
    /// 账户用户名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 账户 Uuid
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// 账户 AccessToken
    /// </summary>
    public string AccessToken { get; set; }
}

public record MicrosoftAccount : Account
{
    public override AccountType Type => AccountType.Microsoft;

    public string RefreshToken { get; set; }

    public DateTime LastRefreshTime { get; set; }
}

public record YggdrasilAccount : Account
{
    public override AccountType Type => AccountType.Yggdrasil;

    public string YggdrasilServerUrl { get; set; }
}

public record OfflineAccount : Account
{
    public override AccountType Type => AccountType.Offline;
}

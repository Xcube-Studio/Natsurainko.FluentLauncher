using Nrk.FluentCore.Classes.Enums;
using System;

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

/// <summary>
/// 微软账户
/// </summary>
public record MicrosoftAccount : Account
{
    public override AccountType Type => AccountType.Microsoft;

    public string RefreshToken { get; set; }

    /// <summary>
    /// 最后一次刷新时间
    /// </summary>
    public DateTime LastRefreshTime { get; set; }
}

/// <summary>
/// 外置账户
/// </summary>
public record YggdrasilAccount : Account
{
    public override AccountType Type => AccountType.Yggdrasil;

    /// <summary>
    /// 外置验证服务器Url
    /// </summary>
    public string YggdrasilServerUrl { get; set; }
}

/// <summary>
/// 离线账户
/// </summary>
public record OfflineAccount : Account
{
    public override AccountType Type => AccountType.Offline;
}

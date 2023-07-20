using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Enums;

/// <summary>
/// 游戏账户类型
/// </summary>
public enum AccountType
{
    /// <summary>
    /// 离线账户
    /// </summary>
    Offline = 0,
    /// <summary>
    /// 微软账户
    /// </summary>
    Microsoft = 1,
    /// <summary>
    /// Yggdrasil 第三方账户
    /// </summary>
    Yggdrasil = 2
}

using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Components.Authenticate;

/// <summary>
/// 验证器的抽象定义
/// </summary>
/// <typeparam name="TAccount"></typeparam>
public abstract class BaseAuthenticator<TAccount> : IAuthenticator<TAccount>
{
    /// <summary>
    /// 验证并取回账户
    /// </summary>
    /// <returns></returns>
    public abstract TAccount Authenticate();
}

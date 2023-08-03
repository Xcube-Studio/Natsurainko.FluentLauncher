using Nrk.FluentCore.Interfaces;

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

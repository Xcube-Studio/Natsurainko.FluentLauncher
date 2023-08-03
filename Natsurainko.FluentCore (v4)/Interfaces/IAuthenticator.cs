namespace Nrk.FluentCore.Interfaces;

/// <summary>
/// 验证器接口
/// </summary>
/// <typeparam name="TAccount"></typeparam>
public interface IAuthenticator<out TAccount>
{
    TAccount Authenticate();
}

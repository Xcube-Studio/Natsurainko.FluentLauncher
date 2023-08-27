using System;

namespace Natsurainko.FluentLauncher.Classes.Exceptions;

internal class AuthenticateRefreshAccountException : Exception
{
    public AuthenticateRefreshAccountException(Exception exception) 
        : base("刷新并验证账户令牌失败", innerException: exception)
    {

    }
}

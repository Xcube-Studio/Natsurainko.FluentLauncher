using Nrk.FluentCore.Classes.Enums;
using System;

namespace Nrk.FluentCore.Classes.Exceptions;

public class MicrosoftAuthenticateException : Exception
{
    public MicrosoftAuthenticateException(string message) : base(message)
    {

    }

    public MicrosoftAuthenticateExceptionType Type { get; internal set; }

    public MicrosoftAuthenticateStep Step { get; internal set; }
}

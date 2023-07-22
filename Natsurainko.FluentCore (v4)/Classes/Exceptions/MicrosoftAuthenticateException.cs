﻿using Nrk.FluentCore.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Exceptions;

public class MicrosoftAuthenticateException : Exception
{
    public MicrosoftAuthenticateExceptionType Type { get; internal set; }

    public new string HelpLink { get; internal set; }

    public MicrosoftAuthenticateStep Step { get; internal set; }

    public new string Message { get; internal set; }

    public new Exception InnerException { get; internal set; }
}
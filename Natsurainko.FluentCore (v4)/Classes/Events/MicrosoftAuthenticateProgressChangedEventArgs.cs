using Nrk.FluentCore.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Events;

public class MicrosoftAuthenticateProgressChangedEventArgs
{
    public MicrosoftAuthenticateStep AuthenticateStep { get; set; }

    public double Progress { get; set; }

    public static implicit operator MicrosoftAuthenticateProgressChangedEventArgs((MicrosoftAuthenticateStep, double) value) => new()
    {
        AuthenticateStep = value.Item1,
        Progress = value.Item2
    };
}

using Natsurainko.FluentCore.Class.Model.Launch;
using System;

namespace Natsurainko.FluentCore.Class.Model.Install;

public class InstallerResponse
{
    public bool Success { get; set; }

    public GameCore GameCore { get; set; }

    public Exception Exception { get; set; }
}

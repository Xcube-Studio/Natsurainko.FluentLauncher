using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace Natsurainko.FluentCore.Event;

public class MinecraftExitedArgs
{
    public int ExitCode { get; set; }

    public bool Crashed { get; set; }

    [JsonIgnore]
    public Stopwatch RunTime { get; set; }

    public IEnumerable<string> Outputs { get; set; }
}

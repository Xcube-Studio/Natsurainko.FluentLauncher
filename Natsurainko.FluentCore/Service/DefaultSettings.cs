using System.Collections.Generic;

namespace Natsurainko.FluentCore.Service;

public static class DefaultSettings
{
    public static int DownloadMaxThreadNumber { get; set; } = 512;

    public static readonly IEnumerable<string> DefaultAdvancedArguments = new string[]
    {
        "-XX:-OmitStackTraceInFastThrow",
        "-XX:-DontCompileHugeMethods",
        "-Dfile.encoding=GB18030",
        "-Dfml.ignoreInvalidMinecraftCertificates=true",
        "-Dfml.ignorePatchDiscrepancies=true",
        "-Djava.rmi.server.useCodebaseOnly=true",
        "-Dcom.sun.jndi.rmi.object.trustURLCodebase=false",
        "-Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false"
    };

    public static readonly IEnumerable<string> DefaultGCArguments = new string[]
    {
        "-XX:+UseG1GC",
        "-XX:+UnlockExperimentalVMOptions",
        "-XX:G1NewSizePercent=20",
        "-XX:G1ReservePercent=20",
        "-XX:MaxGCPauseMillis=50",
        "-XX:G1HeapRegionSize=16m",
        "-XX:-UseAdaptiveSizePolicy"
    };
}

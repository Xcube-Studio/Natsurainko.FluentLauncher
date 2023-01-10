using System;

namespace Natsurainko.FluentCore.Class.Model.Install;

public class ModLoaderInformation
{
    public ModLoaderType LoaderType { get; set; }

    public string Version { get; set; }

    public string McVersion { get; set; }

    public string LoaderName => LoaderType.ToString();

    public DateTime? ReleaseTime { get; set; }
}

public enum ModLoaderType
{
    Any = 0,
    Forge = 1,
    Cauldron = 2,
    LiteLoader = 3,
    Fabric = 4,
    OptiFine = 6,
    Unknown = 7,
}

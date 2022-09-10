using System.IO;

namespace Natsurainko.FluentLauncher.Shared.Class.Model;

public class LocalModInformation
{
    public FileInfo FileInfo { get; set; }

    public string FileName { get; set; }

    public ModType ModType { get; set; } = 0;

    public string Name { get; set; }

    public string Description { get; set; }

    public string[] Authors { get; set; }

    public string Version { get; set; }

    public bool Enable { get; set; }
}

public enum ModType
{
    Forge = 0,
    Fabric = 1,
    ForgeAndFabric = 2
}


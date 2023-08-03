using Nrk.FluentCore.Classes.Enums;

namespace Nrk.FluentCore.Classes.Datas;

public record ModInfo
{
    public string AbsolutePath { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string Version { get; set; }

    public string[] Authors { get; set; }

    public bool IsEnabled { get; set; }

    public ModLoaderType[] SupportedModLoaders { get; set; }
}

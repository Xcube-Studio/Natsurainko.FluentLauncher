using Nrk.FluentCore.Classes.Enums;

namespace Nrk.FluentCore.Classes.Datas.Parse;

public record ModLoaderInfo
{
    public ModLoaderType LoaderType { get; set; }

    public string Version { get; set; }
}

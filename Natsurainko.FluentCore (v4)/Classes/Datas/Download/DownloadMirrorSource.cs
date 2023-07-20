using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Datas.Download;

public record DownloadMirrorSource
{
    public required string Domain { get; set; }

    public required string Name { get; set; }

    public required string VersionManifestUrl { get; set; }

    public required Dictionary<string, string> LibrariesReplaceUrl { get; set; }

    public required Dictionary<string, string> AssetsReplaceUrl { get; set; }
}

using Natsurainko.FluentLauncher.Classes.Data.UI;
using Nrk.FluentCore.GameResources.ThirdPartySources;
using System.Collections.Generic;
using static Natsurainko.FluentLauncher.Classes.Data.UI.CoreInstallProcess;

namespace Natsurainko.FluentLauncher.Classes.Data.Download;

internal record CoreInstallationInfo
{
    public string AbsoluteId { get; set; }

    public string NickName { get; set; }

    public bool EnableIndependencyCore { get; set; }

    public ChooseModLoaderData PrimaryLoader { get; set; }

    public ChooseModLoaderData SecondaryLoader { get; set; }

    public VersionManifestItem ManifestItem { get; set; }

    public List<ProgressItem> AdditionalOptions { get; set; } = new();
}

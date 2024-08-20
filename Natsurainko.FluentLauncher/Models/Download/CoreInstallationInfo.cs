using Natsurainko.FluentLauncher.Models.UI;

using System.Collections.Generic;
using static Natsurainko.FluentLauncher.ViewModels.Common.InstallProcessViewModel;

#nullable disable
namespace Natsurainko.FluentLauncher.Models.Download;

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

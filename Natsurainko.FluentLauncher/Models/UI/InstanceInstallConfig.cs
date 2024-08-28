using Natsurainko.FluentLauncher.Services.Network.Data;
using Nrk.FluentCore.GameManagement.Installer;
using System.Collections.Generic;

#nullable disable
namespace Natsurainko.FluentLauncher.Models.UI;

internal class InstanceInstallConfig
{
    public string InstanceId { get; set; }

    public string NickName { get; set; }

    public bool EnableIndependencyInstance { get; set; }

    public ChooseModLoaderData PrimaryLoader { get; set; }

    public ChooseModLoaderData SecondaryLoader { get; set; }

    public VersionManifestItem ManifestItem { get; set; }

    public List<GameResourceFile> AdditionalResources { get; set; } = [];
}

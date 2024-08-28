using Nrk.FluentCore.Experimental.GameManagement.Installer.Data;

#nullable disable
namespace Natsurainko.FluentLauncher.Models.UI;

public class InstanceInstallConfig
{
    public string InstanceId { get; set; }

    public string NickName { get; set; }

    public bool EnableIndependencyInstance { get; set; }

    public ChooseModLoaderData PrimaryLoader { get; set; }

    public ChooseModLoaderData SecondaryLoader { get; set; }

    public VersionManifestItem ManifestItem { get; set; }

    //public List<ProgressItem> AdditionalOptions { get; set; } = new();
}

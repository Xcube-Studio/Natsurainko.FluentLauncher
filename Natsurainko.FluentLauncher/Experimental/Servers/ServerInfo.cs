#nullable disable
namespace Natsurainko.FluentLauncher.Experimental.Servers;

internal record ServerInfo
{
    public string Name { get; set; }

    public string Address { get; set; }

    public string Icon { get; set; }
}

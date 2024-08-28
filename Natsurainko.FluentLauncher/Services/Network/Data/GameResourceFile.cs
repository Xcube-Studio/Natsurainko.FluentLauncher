using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Network.Data;

internal class GameResourceFile
{
    private readonly Task<string> getUrl;

    public GameResourceFile(Task<string> func) => getUrl = func;

    public required string[] Loaders { get; set; }

    public required string FileName { get; set; }

    public required string Version { get; set; }

    public Task<string> GetUrl() => getUrl;
}

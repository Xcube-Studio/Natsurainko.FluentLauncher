using Natsurainko.FluentCore.Module.Mod;
using Natsurainko.FluentLauncher.Services.Data;
using System.Linq;
using System.Threading.Tasks;
using Natsurainko.Toolkits.Values;

namespace Natsurainko.FluentLauncher.Services;

internal class CurseForgeModService
{
    public CurseForgeModService() => CurseForgeApi.InitApiKey("$2a$10$Awb53b9gSOIJJkdV3Zrgp.CyFP.dI13QKbWn/4UZI4G4ff18WneB6");

    public async Task<CurseForgeCategoryData[]> GetCurseForgeCategoriesAsync()
    {
        return (await CurseForgeApi.GetCategoriesMain())
            .Select(x => new CurseForgeCategoryData(x.Name, x.Id, x.IconUrl))
            .ToArray();
    }

    public async Task<CurseForgeResourceData[]> GetFeaturedResourcesAsync()
    {
        return (await CurseForgeApi.GetFeaturedResources())
            .Select(x => new CurseForgeResourceData(
                x,
                x.Name,
                x.Summary,
                string.Join(", ", x.Author.Select(x => x.Name)),
                x.DateModified,
                x.DownloadCount.FormatUnit(),
                x.Id,
                x.Logo.Url))
            .ToArray();
    }

    public async Task<string[]> GetVersionsAsync()
    {
        return new string[] { "All" }
            .Union(await CurseForgeApi.GetMinecraftVersions())
            .ToArray();
    }

    public async Task<CurseForgeResourceData[]> SearchResourcesAsync(string name, string gameVersion, int categoryId)
    {
        return (await CurseForgeApi.SearchResources(name, gameVersion: gameVersion, categoryId: categoryId))
            .Select(x => new CurseForgeResourceData(
                x,
                x.Name,
                x.Summary,
                string.Join(", ", x.Author.Select(x => x.Name)),
                x.DateModified,
                x.DownloadCount.FormatUnit(),
                x.Id,
                x.Logo.Url))
            .ToArray();
    }
}

using Natsurainko.FluentLauncher.Shared.Class;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Desktop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
#endif

namespace Natsurainko.FluentLauncher.Shared.Mapping;

public static class LocalModManager
{
#if WINDOWS_UWP

    public static async Task<List<LocalModInformation>> GetModsOfGameCore(string folder, string id, bool enableIndependencyCore)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .AddParameter(id)
            .AddParameter(enableIndependencyCore)
            .SetMethod("GetModsOfGameCore");

        return (await DesktopServiceManager.Service.SendAsync<List<LocalModInformation>>(builder.Build())).Response;
    }

    public static async Task<bool> SwitchModOfGameCore(LocalModInformation modInformation)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(modInformation)
            .SetMethod("SwitchModOfGameCore");

        return (await DesktopServiceManager.Service.SendAsync<bool>(builder.Build())).Response;
    }

    public static async Task<bool> DeleteModOfGameCore(LocalModInformation modInformation)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(modInformation)
            .SetMethod("DeleteModOfGameCore");

        return (await DesktopServiceManager.Service.SendAsync<bool>(builder.Build())).Response;
    }

#endif

#if NETCOREAPP
    public static List<LocalModInformation> GetModsOfGameCore(string folder, string id, bool enableIndependencyCore)
    {
        var modsFolder = new DirectoryInfo(enableIndependencyCore ? Path.Combine(folder, "versions", id, "mods") : Path.Combine(folder, "mods"));
        if (!modsFolder.Exists)
            return new();

        var modParser = new LocalModFinder(modsFolder);
        return modParser.GetLocalModInformations().ToList();
    }

    public static bool SwitchModOfGameCore(LocalModInformation localModInformation)
    {
        try
        {
            if (localModInformation.FileInfo.Extension.Equals(".jar"))
                localModInformation.FileInfo.MoveTo(localModInformation.FileInfo.FullName.Replace(".jar", ".disabled"));
            else localModInformation.FileInfo.MoveTo(localModInformation.FileInfo.FullName.Replace(".disabled", ".jar"));

            return true;
        }
        catch { return false; }
    }

    public static bool DeleteModOfGameCore(LocalModInformation localModInformation)
    {
        try
        {
            localModInformation.FileInfo.Delete();

            return true;
        }
        catch { return false; }
    }
#endif
}

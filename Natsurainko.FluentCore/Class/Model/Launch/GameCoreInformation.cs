using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Module.Downloader;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Class.Model.Launch;

public class GameCoreInformation
{
    public List<ModLoaderInformation> ModLoaders { get; set; } = new();

    public int LibraryCount { get; set; }

    public int AssetCount { get; set; }

    public long TotalSize { get; set; } = 0;

    public static async Task<GameCoreInformation> CreateFromGameCore(GameCore core)
    {
        var info = new GameCoreInformation();

        #region GetModLoaders
        try
        {
            core.BehindArguments.ToList().ForEach(x =>
            {
                switch (x)
                {
                    case "--tweakClass optifine.OptiFineTweaker":
                        foreach (var lib in core.LibraryResources)
                            if (lib.Name.ToLower().StartsWith("optifine:optifine"))
                            {
                                var id = lib.Name.Split(':')[2];

                                info.ModLoaders.Add(new ModLoaderInformation
                                {
                                    LoaderType = ModLoaderType.OptiFine,
                                    Version = id.Substring(id.IndexOf('_') + 1),
                                });

                                break;
                            }
                        break;
                    case "--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker":
                    case "--fml.forgeGroup net.minecraftforge":
                        foreach (var lib in core.LibraryResources)
                            if (lib.Name.StartsWith("net.minecraftforge:forge:") || lib.Name.StartsWith("net.minecraftforge:fmlloader:"))
                            {
                                info.ModLoaders.Add(new ModLoaderInformation
                                {
                                    LoaderType = ModLoaderType.Forge,
                                    Version = lib.Name.Split(':')[2].Split('-')[1]
                                });

                                break;
                            }
                        break;
                }
            });
        }
        catch { }

        try
        {
            core.FrontArguments.ToList().ForEach(x =>
            {
                if (x.Contains("-DFabricMcEmu= net.minecraft.client.main.Main"))
                    foreach (var lib in core.LibraryResources)
                        if (lib.Name.StartsWith("net.fabricmc:fabric-loader"))
                        {
                            info.ModLoaders.Add(new ModLoaderInformation
                            {
                                LoaderType = ModLoaderType.Fabric,
                                Version = lib.Name.Split(':')[2]
                            });

                            break;
                        }
            });
        }
        catch { }
        #endregion

        #region GetFileProperty

        info.LibraryCount = core.LibraryResources.Count;

        foreach (var library in core.LibraryResources)
        {
            if (library.Size != 0)
                info.TotalSize += library.Size;
            else if (library.Size == 0 && library.ToFileInfo().Exists)
                info.TotalSize += library.ToFileInfo().Length;
        }

        try
        {
            var assets = await new ResourceDownloader(core).GetAssetResourcesAsync();

            info.AssetCount = assets.Count;

            foreach (var asset in assets)
            {
                if (asset.Size != 0)
                    info.TotalSize += asset.Size;
                else if (asset.Size == 0 && asset.ToFileInfo().Exists)
                    info.TotalSize += asset.ToFileInfo().Length;
            }
        }
        catch { }

        #endregion

        return info;
    }
}

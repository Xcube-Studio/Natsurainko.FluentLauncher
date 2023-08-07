using Nrk.FluentCore.Classes.Datas.Download;
using System.Collections.Generic;

namespace Nrk.FluentCore.DefaultComponents.Download;

public static class DownloadMirrors
{
    public readonly static DownloadMirrorSource Bmclapi = new()
    {
        Name = "Bmclapi",
        Domain = "https://bmclapi2.bangbang93.com",
        VersionManifestUrl = "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        AssetsReplaceUrl = new Dictionary<string, string>()
        {
            { "https://resources.download.minecraft.net", "https://bmclapi2.bangbang93.com/assets" },
            { "https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com" },
            { "https://launchermeta.mojang.com", "https://bmclapi2.bangbang93.com" },
        },
        LibrariesReplaceUrl = new Dictionary<string, string>()
        {
            { "https://launcher.mojang.com" , "https://bmclapi2.bangbang93.com" },
            { "https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven" },
            { "https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com" },
            { "https://maven.minecraftforge.net", "https://bmclapi2.bangbang93.com/maven" },
            { "https://files.minecraftforge.net/maven", "https://bmclapi2.bangbang93.com/maven" },
            { "https://maven.fabricmc.net", "https://bmclapi2.bangbang93.com/maven" },
            { "https://meta.fabricmc.net", "https://bmclapi2.bangbang93.com/fabric-meta" },
        }
    };

    public readonly static DownloadMirrorSource Mcbbs = new()
    {
        Name = "Mcbbs",
        Domain = "https://download.mcbbs.net",
        VersionManifestUrl = "https://download.mcbbs.net/mc/game/version_manifest_v2.json",
        AssetsReplaceUrl = new Dictionary<string, string>()
        {
            { "https://resources.download.minecraft.net", "https://download.mcbbs.net/assets" },
            { "https://piston-meta.mojang.com", "https://download.mcbbs.net" },
            { "https://launchermeta.mojang.com", "https://download.mcbbs.net" },
        },
        LibrariesReplaceUrl = new Dictionary<string, string>()
        {
            { "https://launcher.mojang.com" , "https://download.mcbbs.net" },
            { "https://libraries.minecraft.net", "https://download.mcbbs.net/maven" },
            { "https://piston-meta.mojang.com", "https://download.mcbbs.net" },
            { "https://maven.minecraftforge.net", "https://download.mcbbs.net/maven" },
            { "https://files.minecraftforge.net/maven", "https://download.mcbbs.net/maven" },
            { "https://maven.fabricmc.net", "https://download.mcbbs.net/maven" },
            { "https://meta.fabricmc.net", "https://download.mcbbs.net/fabric-meta" },
        }
    };
}

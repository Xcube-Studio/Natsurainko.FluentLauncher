using System.Collections.Generic;

namespace Natsurainko.FluentCore.Service;

public static class DownloadApiManager
{
    public static DownloadApi Current { get; set; } = new DownloadApi
    {
        Host = "https://download.mcbbs.net",
        VersionManifest = "https://download.mcbbs.net/mc/game/version_manifest.json",
        Assets = "https://download.mcbbs.net/assets",
        Libraries = "https://download.mcbbs.net/maven"
    };

    public static readonly DownloadApi Mojang = new()
    {
        Host = "https://launcher.mojang.com",
        VersionManifest = "http://launchermeta.mojang.com/mc/game/version_manifest.json",
        Assets = "http://resources.download.minecraft.net",
        Libraries = "https://libraries.minecraft.net"
    };

    public static readonly DownloadApi Bmcl = new()
    {
        Host = "https://bmclapi2.bangbang93.com",
        VersionManifest = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json",
        Assets = "https://bmclapi2.bangbang93.com/assets",
        Libraries = "https://bmclapi2.bangbang93.com/maven"
    };

    public static readonly DownloadApi Mcbbs = new()
    {
        Host = "https://download.mcbbs.net",
        VersionManifest = "https://download.mcbbs.net/mc/game/version_manifest.json",
        Assets = "https://download.mcbbs.net/assets",
        Libraries = "https://download.mcbbs.net/maven"
    };

    public static readonly Dictionary<string, string> ForgeLibraryUrlReplace = new()
    {
        { "https://maven.minecraftforge.net", $"{(Current.Host.Equals(Mojang.Host) ? "https://maven.minecraftforge.net" : Current.Libraries )}" },
        { "https://files.minecraftforge.net/maven", $"{(Current.Host.Equals(Mojang.Host) ? "https://maven.minecraftforge.net" : Current.Libraries )}" }
    };

    public static readonly Dictionary<string, string> FabricLibraryUrlReplace = new()
    {
        { "https://maven.fabricmc.net", $"{(Current.Host.Equals(Mojang.Host) ? "https://maven.fabricmc.net" : Current.Libraries )}" },
        { "https://meta.fabricmc.net", $"{(Current.Host.Equals(Mojang.Host) ? "https://meta.fabricmc.net" : $"{Current.Host}/fabric-meta" )}" }
    };
}

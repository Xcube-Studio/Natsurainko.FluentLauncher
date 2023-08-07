using Nrk.FluentCore.Classes.Data.Launch;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.DefaultComponents.Parse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nrk.FluentCore.Utils;

public static class GameInfoExtensions
{
    public static LibraryElement GetJarElement(this GameInfo gameInfo)
    {
        var jsonClient = JsonNode.Parse(File.ReadAllText(gameInfo.IsInheritedFrom ? gameInfo.InheritsFrom.VersionJsonPath : gameInfo.VersionJsonPath))
            ?["downloads"]?["client"];

        if (jsonClient != null)
            return new LibraryElement
            {
                AbsolutePath = gameInfo.IsInheritedFrom ? gameInfo.InheritsFrom.JarPath : gameInfo.JarPath,
                Checksum = jsonClient["sha1"].GetValue<string>(),
                Url = jsonClient["url"].GetValue<string>()
            };

        return null;
    }

    public static string GetSuitableJavaVersion(this GameInfo gameInfo)
    {
        if (gameInfo.IsInheritedFrom)
            return GetSuitableJavaVersion(gameInfo.InheritsFrom);

        var jsonMajorVersion = JsonNode.Parse(File.ReadAllText(gameInfo.VersionJsonPath))["javaVersion"]?["majorVersion"];

        if (jsonMajorVersion != null) return jsonMajorVersion.GetValue<int>().ToString();

        return "8";
    }

    public static IEnumerable<ModLoaderInfo> GetModLoaders(this GameInfo gameInfo)
    {
        var handle = new Dictionary<string, (ModLoaderType, Func<string, string>)>()
        {
            { "net.minecraftforge:forge:", (ModLoaderType.Forge, libVersion => libVersion.Split('-')[1]) },
            { "net.minecraftforge:fmlloader:", (ModLoaderType.Forge, libVersion => libVersion.Split('-')[1]) },
            { "optifine:optifine", (ModLoaderType.OptiFine, libVersion => libVersion[(libVersion.IndexOf('_') + 1)..]) },
            { "net.fabricmc:fabric-loader", (ModLoaderType.Fabric, libVersion => libVersion) },
            { "com.mumfrey:liteloader:", (ModLoaderType.LiteLoader, libVersion => libVersion) },
            { "org.quiltmc:quilt-loader:", (ModLoaderType.Quilt, libVersion => libVersion) },
        };

        var libraryJsonNodes = JsonNode.Parse(File.ReadAllText(gameInfo.VersionJsonPath))["libraries"].Deserialize<IEnumerable<LibraryJsonNode>>();
        var enumed = new List<ModLoaderInfo>();

        foreach (var library in libraryJsonNodes)
        {
            var loweredName = library.Name.ToLower();

            foreach (var key in handle.Keys)
            {
                if (!loweredName.Contains(key))
                    continue;

                var id = loweredName.Split(':')[2];
                var loader = new ModLoaderInfo { LoaderType = handle[key].Item1, Version = handle[key].Item2(id) };

                if (enumed.Contains(loader)) break;

                yield return loader;
                enumed.Add(loader);

                break;
            }
        }
    }

    public static GameStatisticInfo GetStatisticInfo(this GameInfo gameInfo)
    {
        var libraryParser = new DefaultLibraryParser(gameInfo);
        libraryParser.EnumerateLibraries(out var enabledLibraries, out var enabledNativesLibraries);

        var assetParser = new DefaultAssetParser(gameInfo);

        long length = 0;
        int assets = 0;

        foreach (var library in enabledLibraries)
        {
            if (File.Exists(library.AbsolutePath))
                length += new FileInfo(library.AbsolutePath).Length;
        }

        foreach (var library in enabledNativesLibraries)
        {
            if (File.Exists(library.AbsolutePath))
                length += new FileInfo(library.AbsolutePath).Length;
        }

        if (File.Exists(gameInfo.AssetsIndexJsonPath))
        {
            foreach (var asset in assetParser.EnumerateAssets())
            {
                assets++;

                if (File.Exists(asset.AbsolutePath))
                    length += new FileInfo(asset.AbsolutePath).Length;
            }

            length += new FileInfo(gameInfo.AssetsIndexJsonPath).Length;
        }

        if (File.Exists(gameInfo.JarPath))
            length += new FileInfo(gameInfo.JarPath).Length;

        length += new FileInfo(gameInfo.VersionJsonPath).Length;

        return new GameStatisticInfo
        {
            AssetsCount = assets,
            LibrariesCount = enabledLibraries.Count,
            TotalSize = length,
            ModLoaders = GetModLoaders(gameInfo)
        };
    }
}

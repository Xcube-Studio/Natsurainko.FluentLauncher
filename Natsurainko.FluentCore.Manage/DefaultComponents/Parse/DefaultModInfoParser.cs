using Nrk.FluentCore.Classes.Datas;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using Tomlyn;
using Tomlyn.Model;

namespace Nrk.FluentCore.DefaultComponents.Parse;

public static class DefaultModInfoParser
{
    public static ModInfo Parse(string filePath)
    {
        var supportedModLoaders = new List<ModLoaderType>();

        var modInfo = new ModInfo
        {
            AbsolutePath = filePath,
            IsEnabled = Path.GetExtension(filePath).Equals(".jar")
        };

        using var zipArchive = ZipFile.OpenRead(filePath);

        var quiltModJson = zipArchive.GetEntry("quilt.mod.json");
        var fabricModJson = zipArchive.GetEntry("fabric.mod.json");
        var modsToml = zipArchive.GetEntry("META-INF/mods.toml");
        var mcmodInfo = zipArchive.GetEntry("mcmod.info");

        if (quiltModJson != null) supportedModLoaders.Add(ModLoaderType.Quilt);
        if (fabricModJson != null) supportedModLoaders.Add(ModLoaderType.Fabric);
        if (modsToml != null || mcmodInfo != null) supportedModLoaders.Add(ModLoaderType.Forge);

        if (!supportedModLoaders.Any()) supportedModLoaders.Add(ModLoaderType.Unknown);
        modInfo.SupportedModLoaders = supportedModLoaders.ToArray();

        if (quiltModJson != null) return ParseModJson(ref modInfo, quiltModJson.ReadAsString(), true);
        if (fabricModJson != null) return ParseModJson(ref modInfo, fabricModJson.ReadAsString(), false);

        if (modsToml != null) return ParseModsToml(ref modInfo, modsToml.ReadAsString());
        if (mcmodInfo != null) return ParseMcmodInfo(ref modInfo, mcmodInfo.ReadAsString());

        throw new Exception("Unknown Mod Type");
    }

    private static ModInfo ParseModJson(ref ModInfo mod, string jsonContent, bool isQuilt)
    {
        var jsonNode = JsonNode.Parse(jsonContent);
        if (isQuilt) jsonNode = jsonNode["quilt_loader"]["metadata"];

        mod.DisplayName = jsonNode["name"].GetValue<string>();
        mod.Version = jsonNode["version"]?.GetValue<string>();
        mod.Description = jsonNode["description"]?.GetValue<string>();
        mod.Authors = jsonNode["authors"].AsArray().Select(x => x.GetValue<string>()).ToArray();

        return mod;
    }

    private static ModInfo ParseModsToml(ref ModInfo mod, string tomlContent)
    {
        var tomlTable = (Toml.ToModel(tomlContent)["mods"] as TomlTableArray).First();

        mod.DisplayName = tomlTable.GetString("displayName");
        mod.Version = tomlTable.GetString("version");
        mod.Description = tomlTable.GetString("description");
        mod.Authors = tomlTable.GetString("authors")?.Split(",").Select(x => x.Trim(' ')).ToArray();

        return mod;
    }

    private static ModInfo ParseMcmodInfo(ref ModInfo mod, string jsonContent)
    {
        var jsonNode = JsonNode.Parse(jsonContent.Replace("\u000a", "")).AsArray().FirstOrDefault()
            ?? throw new InvalidDataException("Invalid mcmod.info");

        mod.DisplayName = jsonNode["name"].GetValue<string>();
        mod.Version = jsonNode["version"]?.GetValue<string>();
        mod.Description = jsonNode["description"]?.GetValue<string>();
        mod.Authors = (jsonNode["authorList"] ?? jsonNode["authors"])?.AsArray().Select(x => x.GetValue<string>()).ToArray();

        return mod;
    }
}

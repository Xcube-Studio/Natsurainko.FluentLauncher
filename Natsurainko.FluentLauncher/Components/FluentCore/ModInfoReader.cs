using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Model.Install;
using Natsurainko.Toolkits.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

public partial class ModInfo : ObservableObject
{
    public FileInfo File { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Version { get; set; }

    public IEnumerable<string> Authors { get; set; }

    public IEnumerable<ModLoaderType> ModLoaders { get; set; }

    [ObservableProperty]
    public bool isEnable;

    [ObservableProperty]
    public string displayAuthors;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsEnable))
        {
            var fileName = Path.Combine(
                File.DirectoryName,
                Path.GetFileNameWithoutExtension(File.FullName) + (IsEnable ? ".jar" : ".disabled"));

            if (!File.FullName.Equals(fileName))
                File.MoveTo(fileName);
        }
    }
}

public class ModInfoReader
{
    private static readonly Dictionary<string, Func<ZipArchive, ModInfo>> Actions = new();

    static ModInfoReader()
    {
        Actions.Add("quilt.mod.json", HandleQuiltModJson);
        Actions.Add("fabric.mod.json", HandleFabricModJson);
        Actions.Add("META-INF/mods.toml", HandleModsToml);
        Actions.Add("mcmod.info", HandleMcmodInfo);
    }

    private static ModInfo HandleQuiltModJson(ZipArchive zipArchive)
    {
        string jsonContent = zipArchive.GetEntry("quilt.mod.json").GetString();

        var keyValuePairs = JObject.Parse(jsonContent)["quilt_loader"];

        return new ModInfo
        {
            ModLoaders = new ModLoaderType[] { ModLoaderType.Quilt },
            Authors = keyValuePairs["metadata"]["contributors"]
                .ToObject<Dictionary<string, string>>()
                .Where(x => x.Value.Equals("Owner"))
                .Select(x => x.Key),
            Name = keyValuePairs["metadata"]["name"].ToString(),
            Version = keyValuePairs["version"].ToString(),
            Description = keyValuePairs["metadata"]["description"].ToString()
        };
    }

    private static ModInfo HandleFabricModJson(ZipArchive zipArchive)
    {
        string jsonContent = zipArchive.GetEntry("fabric.mod.json").GetString();

        var keyValuePairs = JObject.Parse(jsonContent);

        return new ModInfo
        {
            ModLoaders = new ModLoaderType[] { ModLoaderType.Fabric },
            Authors = ((IDictionary<string, JToken>)keyValuePairs).Keys
                .Where(x => x.Contains("author"))
                .Select(x => keyValuePairs[x].ToArray().Select(x => x.Type.Equals(JTokenType.Object)
                    ? x["name"].ToString()
                    : x.ToString()))
                .FirstOrDefault(Array.Empty<string>()),
            Name = keyValuePairs["name"].ToString(),
            Version = keyValuePairs["version"].ToString(),
            Description = keyValuePairs["description"].ToString()
        };
    }

    private static ModInfo HandleModsToml(ZipArchive zipArchive)
    {
        string tomlContent = zipArchive.GetEntry("META-INF/mods.toml").GetString();

        var keyValuePairs = (Toml.ToModel(tomlContent)["mods"] as TomlTableArray).First();

        return new ModInfo
        {
            ModLoaders = new ModLoaderType[] { ModLoaderType.Forge },
            Authors = keyValuePairs.ContainsKey("authors")
                ? GetStringFromTomlTable(keyValuePairs, "authors").Split(",").Select(x => x.Trim(' ')).ToArray()
                : Array.Empty<string>(),
            Name = GetStringFromTomlTable(keyValuePairs, "displayName"),
            Version = GetStringFromTomlTable(keyValuePairs, "version"),
            Description = GetStringFromTomlTable(keyValuePairs, "description")
        };
    }

    private static ModInfo HandleMcmodInfo(ZipArchive zipArchive)
    {
        string jsonContent = zipArchive.GetEntry("mcmod.info").GetString();

        var jToken = JToken.Parse(jsonContent);
        var keyValuePairs = (jToken.Type == JTokenType.Array
            ? jToken.ToArray()[0]
            : jToken["modList"].ToArray()[0]) as JObject;

        return new ModInfo
        {
            ModLoaders = new ModLoaderType[] { ModLoaderType.Forge },
            Authors = ((IDictionary<string, JToken>)keyValuePairs).Keys
                .Where(x => x.Contains("author"))
                .Select(x => keyValuePairs[x].ToArray().Select(x => x.Type == JTokenType.Object
                    ? x["name"].ToString()
                    : x.ToString()))
                .FirstOrDefault(Array.Empty<string>()),
            Name = keyValuePairs["name"].ToString(),
            Version = keyValuePairs["version"].ToString(),
            Description = keyValuePairs["description"].ToString()
        };
    }

    public static IEnumerable<ModInfo> GetModInfos(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles().Where(x => x.Extension.Equals(".jar") || x.Extension.Equals(".disabled")))
        {
            using var zipArchive = ZipFile.OpenRead(file.FullName);

            var actions = new Dictionary<string, Func<ZipArchive, ModInfo>>();
            var modInfos = new List<ModInfo>();

            foreach (var kvp in Actions)
                if (zipArchive.GetEntry(kvp.Key) != null)
                    modInfos.Add(kvp.Value(zipArchive));

            if (modInfos.Any())
            {
                var modInfo = modInfos.First();

                modInfo.File = file;
                modInfo.isEnable = !file.Extension.Equals(".disabled");
                modInfo.displayAuthors = string.Join(", ", modInfo.Authors);

                modInfos.ForEach(x => modInfo.ModLoaders = modInfo.ModLoaders.Union(x.ModLoaders));
                yield return modInfo;
            }
        }
    }

    public static string GetStringFromTomlTable(TomlTable keyValuePairs, string key)
    {
        if (!keyValuePairs.ContainsKey(key))
            return string.Empty;

        return keyValuePairs[key].ToString();
    }
}

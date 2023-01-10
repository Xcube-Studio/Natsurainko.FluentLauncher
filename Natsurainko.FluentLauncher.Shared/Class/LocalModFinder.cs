#if NETCOREAPP

using Natsurainko.FluentLauncher.Shared.Class.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace Natsurainko.FluentLauncher.Shared.Class;

public class LocalModFinder
{
    public DirectoryInfo ModFolder { get; private set; }

    public LocalModFinder(DirectoryInfo modFolder)
    {
        this.ModFolder = modFolder;
    }

    public IEnumerable<LocalModInformation> GetLocalModInformations()
    {
        LocalModInformation GetModInformation(FileInfo file)
        {
            var info = new LocalModInformation();

            info.FileInfo = file;
            info.Enable = file.Extension == ".jar";
            info.FileName = Path.GetFileNameWithoutExtension(file.FullName);

            try
            {
                using var zip = ZipFile.OpenRead(file.FullName);

                if (zip.GetEntry("META-INF/mods.toml") != null)
                {
                    using var contentStream = zip.GetEntry("META-INF/mods.toml").Open();
                    using var reader = new StreamReader(contentStream);

                    var toml = ((TomlTableArray)Toml.Parse(reader.ReadToEnd()).ToModel()["mods"])[0];

                    info.ModType = ModType.Forge;
                    info.Authors = toml.ContainsKey("authors") ? toml["authors"].ToString().Split(",").Select(x => x.Trim(' ')).ToArray() : Array.Empty<string>();
                    info.Name = toml["displayName"].ToString();
                    info.Version = toml["version"].ToString();
                    info.Description = toml["description"].ToString();

                    if (zip.GetEntry("META-INF/MANIFEST.MF") != null)
                    {
                        using var mfContentStream = zip.GetEntry("META-INF/MANIFEST.MF").Open();
                        using var mfReader = new StreamReader(mfContentStream);

                        foreach (var line in mfReader.ReadToEnd().Split("\r\n"))
                            if (line.StartsWith("Implementation-Version: "))
                                info.Version = line.Replace("Implementation-Version: ", string.Empty);
                    }
                }
                
                if (zip.GetEntry("mcmod.info") != null)
                {
                    using var contentStream = zip.GetEntry("mcmod.info").Open();
                    using var reader = new StreamReader(contentStream);

                    var jToken = JToken.Parse(reader.ReadToEnd());
                    JObject jObject = null;

                    if (jToken.Type == JTokenType.Array)
                        jObject = (JObject)jToken.ToArray()[0];
                    else jObject = (JObject)jToken["modList"].ToArray()[0];

                    info.ModType = ModType.Forge;
                    info.Name = jObject["name"].ToString();
                    info.Version = jObject["version"].ToString();
                    info.Description = jObject["description"].ToString();

                    try
                    {
                        if (jObject.ContainsKey("authorList"))
                            info.Authors = ((JArray)jObject["authorList"]).Select(x => x.Type == JTokenType.Object ? x["name"].ToString() : x.ToString()).ToArray();

                        if (jObject.ContainsKey("authors"))
                            info.Authors = ((JArray)jObject["authors"]).Select(x => x.Type == JTokenType.Object ? x["name"].ToString() : x.ToString()).ToArray();
                    } catch { }
                }

                if (zip.GetEntry("fabric.mod.json") != null)
                {
                    using var contentStream = zip.GetEntry("fabric.mod.json").Open();
                    using var reader = new StreamReader(contentStream);

                    var jObject = JObject.Parse(reader.ReadToEnd());

                    info.ModType = ModType.Fabric;
                    info.Authors = ((JArray)jObject["authors"]).Select(x => x.Type == JTokenType.Object ? x["name"].ToString() : x.ToString()).ToArray();
                    info.Name = jObject["name"].ToString();
                    info.Version = jObject["version"].ToString();
                    info.Description = jObject["description"].ToString();
                }

                if (zip.GetEntry("fabric.mod.json") != null && zip.GetEntry("META-INF/mods.toml") != null)
                    info.ModType = ModType.ForgeAndFabric;
            }
            catch (Exception ex) 
            { 

            }

            return info;
        }

        foreach (var file in this.ModFolder.EnumerateFiles())
        {
            if (file.Extension.ToLower() != ".jar" && file.Extension.ToLower() != ".disabled")
                continue;

            var info = GetModInformation(file);

            if (info != null)
                yield return info;
        }
    }
}

#endif
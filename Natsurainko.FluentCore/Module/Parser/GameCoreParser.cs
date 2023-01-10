using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Service;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentCore.Module.Parser;

public class GameCoreParser
{
    public DirectoryInfo Root { get; set; }

    public IEnumerable<VersionJsonEntity> JsonEntities { get; set; }

    public GameCoreParser(DirectoryInfo root, IEnumerable<VersionJsonEntity> jsonEntities)
    {
        this.Root = root;
        this.JsonEntities = jsonEntities;
    }

    public List<(string, Exception)> ErrorGameCores { get; private set; } = new();

    public IEnumerable<GameCore> GetGameCores()
    {
        var cores = new List<GameCore>();

        foreach (var entity in JsonEntities)
        {
            try
            {
                var core = new GameCore
                {
                    Id = entity.Id,
                    Type = entity.Type,
                    MainClass = entity.MainClass,
                    InheritsFrom = entity.InheritsFrom,
                    JavaVersion = (int)(entity.JavaVersion?.MajorVersion),
                    LibraryResources = new LibraryParser(entity.Libraries, this.Root).GetLibraries().ToList(),
                    Root = this.Root
                };

                if (string.IsNullOrEmpty(entity.InheritsFrom) && entity.Downloads != null)
                    core.ClientFile = GetClientFile(entity);

                if (string.IsNullOrEmpty(entity.InheritsFrom) && entity.Logging != null && entity.Logging.Client != null)
                    core.LogConfigFile = GetLogConfigFile(entity);

                if (string.IsNullOrEmpty(entity.InheritsFrom) && entity.AssetIndex != null)
                    core.AssetIndexFile = GetAssetIndexFile(entity);

                if (entity.MinecraftArguments != null)
                    core.BehindArguments = HandleMinecraftArguments(entity.MinecraftArguments);

                if (entity.Arguments != null && entity.Arguments.Game != null)
                    core.BehindArguments = core.BehindArguments == null
                        ? HandleArgumentsGame(entity.Arguments)
                        : core.BehindArguments.Union(HandleArgumentsGame(entity.Arguments));

                if (entity.Arguments != null && entity.Arguments.Jvm != null)
                    core.FrontArguments = HandleArgumentsJvm(entity.Arguments);
                else core.FrontArguments = new string[]
                {
                    "-Djava.library.path=${natives_directory}",
                    "-Dminecraft.launcher.brand=${launcher_name}",
                    "-Dminecraft.launcher.version=${launcher_version}",
                    "-cp ${classpath}"
                };

                cores.Add(core);
            }
            catch (Exception ex)
            {
                ErrorGameCores.Add((entity.Id, ex));
            }
        }

        foreach (var item in cores)
        {
            item.Source = GetSource(item);
            item.HasModLoader = GetHasModLoader(item);

            if (string.IsNullOrEmpty(item.InheritsFrom))
                yield return item;
            else
            {
                GameCore inheritsFrom = default;

                foreach (var subitem in cores)
                    if (subitem.Id == item.InheritsFrom)
                        inheritsFrom = subitem;

                if (inheritsFrom == null)
                    continue;
                else yield return Combine(item, inheritsFrom);
            }
        }
    }

    private FileResource GetClientFile(VersionJsonEntity entity)
    {
        var path = Path.Combine(this.Root.FullName, "versions", entity.Id, $"{entity.Id}.jar");

        return new FileResource
        {
            CheckSum = entity.Downloads["client"].Sha1,
            Size = entity.Downloads["client"].Size,
            Url = DownloadApiManager.Current != DownloadApiManager.Mojang ? entity.Downloads["client"].Url.Replace("https://launcher.mojang.com", DownloadApiManager.Current.Host) : entity.Downloads["client"].Url,
            Root = this.Root,
            FileInfo = new FileInfo(path),
            Name = Path.GetFileName(path)
        };
    }

    private FileResource GetLogConfigFile(VersionJsonEntity entity)
    {
        var path = Path.Combine(this.Root.FullName, "versions", entity.Id, entity.Logging.Client.File.Id);

        return new FileResource
        {
            CheckSum = entity.Logging.Client.File.Sha1,
            Size = entity.Logging.Client.File.Size,
            Url = DownloadApiManager.Current != DownloadApiManager.Mojang ? entity.Logging.Client.File.Url.Replace("https://launcher.mojang.com", DownloadApiManager.Current.Host) : entity.Logging.Client.File.Url,
            Name = entity.Logging.Client.File.Id,
            FileInfo = new FileInfo(path),
            Root = this.Root,
        };
    }

    private FileResource GetAssetIndexFile(VersionJsonEntity entity)
    {
        var path = Path.Combine(this.Root.FullName, "assets", "indexes", $"{entity.AssetIndex.Id}.json");

        return new FileResource
        {
            CheckSum = entity.AssetIndex.Sha1,
            Size = entity.AssetIndex.Size,
            Url = DownloadApiManager.Current != DownloadApiManager.Mojang ? entity.AssetIndex.Url.Replace("https://launchermeta.mojang.com", DownloadApiManager.Current.Host).Replace("https://piston-meta.mojang.com", DownloadApiManager.Current.Host) : entity.AssetIndex.Url,
            Name = $"{entity.AssetIndex.Id}.json",
            FileInfo = new FileInfo(path),
            Root = this.Root,
        };
    }

    private string GetSource(GameCore core)
    {
        try
        {
            if (core.InheritsFrom != null)
                return core.InheritsFrom;

            var json = Path.Combine(core.Root.FullName, "versions", core.Id, $"{core.Id}.json");

            if (File.Exists(json))
            {
                var entity = JObject.Parse(File.ReadAllText(json));

                if (entity.ContainsKey("patches"))
                    return ((JArray)entity["patches"])[0]["version"].ToString();

                if (entity.ContainsKey("clientVersion"))
                    return entity["clientVersion"].ToString();
            }
        }
        catch //(Exception ex)
        {
            //throw;
        }

        return core.Id;
    }

    private bool GetHasModLoader(GameCore core)
    {
        foreach (var arg in core.BehindArguments)
            switch (arg)
            {
                case "--tweakClass optifine.OptiFineTweaker":
                case "--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker":
                case "--fml.forgeGroup net.minecraftforge":
                    return true;
            }

        foreach (var arg in core.FrontArguments)
            if (arg.Contains("-DFabricMcEmu= net.minecraft.client.main.Main"))
                return true;

        return core.MainClass switch
        {
            "net.minecraft.client.main.Main" or "net.minecraft.launchwrapper.Launch" or "com.mojang.rubydung.RubyDung" => false,
            _ => true,
        };
    }

    private IEnumerable<string> HandleMinecraftArguments(string minecraftArguments) => ArgumnetsGroup(minecraftArguments.Replace("  ", " ").Split(' '));

    private IEnumerable<string> HandleArgumentsGame(ArgumentsJsonEntity entity) => ArgumnetsGroup(entity.Game.Where(x => x.Type == JTokenType.String).Select(x => x.ToString().ToPath()));

    private IEnumerable<string> HandleArgumentsJvm(ArgumentsJsonEntity entity) => ArgumnetsGroup(entity.Jvm.Where(x => x.Type == JTokenType.String).Select(x => x.ToString().ToPath()));

    private static IEnumerable<string> ArgumnetsGroup(IEnumerable<string> vs)
    {
        var cache = new List<string>();

        foreach (var item in vs)
        {
            if (cache.Any() && cache[0].StartsWith("-") && item.StartsWith("-"))
            {
                yield return cache[0].Trim(' ');

                cache = new List<string> { item };
            }
            else if (vs.Last() == item && !cache.Any())
                yield return item.Trim(' ');
            else cache.Add(item);

            if (cache.Count == 2)
            {
                yield return string.Join(" ", cache).Trim(' ');
                cache = new List<string>();
            }
        }
    }

    private GameCore Combine(GameCore raw, GameCore inheritsFrom)
    {
        raw.AssetIndexFile = inheritsFrom.AssetIndexFile;
        raw.ClientFile = inheritsFrom.ClientFile;
        raw.LogConfigFile = inheritsFrom.LogConfigFile;
        raw.JavaVersion = inheritsFrom.JavaVersion;
        raw.Type = inheritsFrom.Type;
        raw.LibraryResources = raw.LibraryResources.Union(inheritsFrom.LibraryResources).ToList();
        //raw.LibraryResources = CheckRepeat(raw.LibraryResources.Union(inheritsFrom.LibraryResources)).ToList();
        raw.BehindArguments = inheritsFrom.BehindArguments.Union(raw.BehindArguments).ToList();
        raw.FrontArguments = raw.FrontArguments.Union(inheritsFrom.FrontArguments).ToList();

        return raw;
    }
}

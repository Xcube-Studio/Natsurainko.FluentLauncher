using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Model.Parser;
using Natsurainko.FluentCore.Module.Parser;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Values;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

public class GameCoreParser : Natsurainko.FluentCore.Module.Parser.GameCoreParser
{
    public GameCoreParser(DirectoryInfo root, IEnumerable<VersionJsonEntity> jsonEntities) 
        : base(root, jsonEntities) { }

    public override IEnumerable<GameCore> GetGameCores()
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
                    LibraryResources = new LibraryParser(entity.Libraries, Root).GetLibraries().ToList(),
                    Root = Root
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
            catch (Exception ex) { ErrorGameCores.Add((entity.Id, ex)); }
        }

        foreach (var item in cores)
        {
            item.Source = GetSource(item);

            if (!string.IsNullOrEmpty(item.InheritsFrom))
            {
                GameCore inheritsFrom = default;

                foreach (var subitem in cores)
                    if (subitem.Id == item.InheritsFrom)
                        inheritsFrom = subitem;

                if (inheritsFrom != null)
                {
                    item.AssetIndexFile = inheritsFrom.AssetIndexFile;
                    item.ClientFile = inheritsFrom.ClientFile;
                    item.LogConfigFile = inheritsFrom.LogConfigFile;
                    item.JavaVersion = inheritsFrom.JavaVersion;
                    item.Type = inheritsFrom.Type;
                    item.LibraryResources = item.LibraryResources.Union(inheritsFrom.LibraryResources).ToList();
                    item.BehindArguments = inheritsFrom.BehindArguments.Union(item.BehindArguments).ToList();
                    item.FrontArguments = item.FrontArguments.Union(inheritsFrom.FrontArguments).ToList();
                }
                else continue;
            }

            item.ModLoaders = GetModLoaders(item);
            item.IsVanilla = GetIsVanilla(item);
            item.CoreProfile = GetProfile(item);

            yield return item;
        }
    }

    public static CoreProfile GetProfile(GameCore core)
    {
        var file = core.GetFileOfProfile();

        if (!file.Exists)
            return new(file.FullName, core, null, false, null);
        var jObject = JObject.Parse(File.ReadAllText(file.FullName));

        return new(
            file.FullName,
            core,
            jObject["LastLaunchTime"].ToObject<DateTime?>(),
            (bool)jObject["EnableSpecialSetting"],
            jObject["LaunchSetting"]?.ToObject<LaunchSetting>());
    }
}

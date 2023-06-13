using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Parser;
using Natsurainko.Toolkits.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

class GameCoreLocator : IGameCoreLocator<GameCore>
{
    public DirectoryInfo Root { get; private set; }

    public List<(string, Exception)> ErrorGameCores { get; private set; }

    public GameCoreLocator(string path) => Root = new DirectoryInfo(path);

    public GameCoreLocator(DirectoryInfo root) => Root = root;

    public GameCore GetGameCore(string id)
    {
        foreach (var core in GetGameCores())
            if (core.Id == id)
                return core;

        return null;
    }

    public void DeleteGameCore(string id)
    {
        var directory = new DirectoryInfo(Path.Combine(Root.FullName, "versions", id));

        if (directory.Exists)
            directory.DeleteAllFiles();

        directory.Delete();
    }

    public IEnumerable<GameCore> GetGameCores()
    {
        var entities = new List<VersionJsonEntity>();

        var versionsFolder = new DirectoryInfo(Path.Combine(Root.FullName, "versions"));

        if (!versionsFolder.Exists)
            return Array.Empty<GameCore>();

        foreach (var item in versionsFolder.EnumerateDirectories())
            foreach (var files in item.EnumerateFiles())
                if (files.Name == $"{item.Name}.json")
                    try { entities.Add(JsonConvert.DeserializeObject<VersionJsonEntity>(File.ReadAllText(files.FullName))); } catch { }

        var parser = new GameCoreParser(Root, entities);
        var result = parser.GetGameCores();

        ErrorGameCores = parser.ErrorGameCores;

        return result;
    }
}

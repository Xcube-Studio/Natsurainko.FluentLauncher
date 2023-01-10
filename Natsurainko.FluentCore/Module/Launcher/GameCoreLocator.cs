using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Parser;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentCore.Module.Launcher;

public class GameCoreLocator : IGameCoreLocator
{
    public DirectoryInfo Root { get; private set; }

    public List<(string, Exception)> ErrorGameCores { get; private set; }

    public GameCoreLocator(string path) => this.Root = new DirectoryInfo(path);

    public GameCoreLocator(DirectoryInfo root) => this.Root = root;

    public GameCore GetGameCore(string id)
    {
        foreach (var core in this.GetGameCores())
            if (core.Id == id)
                return core;

        return null;
    }

    public void DeleteGameCore(string id)
    {
        var directory = new DirectoryInfo(Path.Combine(this.Root.FullName, "versions", id));

        if (directory.Exists)
            directory.DeleteAllFiles();

        directory.Delete();
    }

    public IEnumerable<GameCore> GetGameCores()
    {
        var entities = new List<VersionJsonEntity>();

        var versionsFolder = new DirectoryInfo(Path.Combine(this.Root.FullName, "versions"));

        if (!versionsFolder.Exists)
        {
            versionsFolder.Create();
            return Array.Empty<GameCore>();
        }

        foreach (var item in versionsFolder.GetDirectories())
            foreach (var files in item.GetFiles())
                if (files.Name == $"{item.Name}.json")
                {
                    var entity = new VersionJsonEntity();
                    try
                    {
                        entity = entity.FromJson(File.ReadAllText(files.FullName));
                        entities.Add(entity);
                    }
                    catch { }
                }

        var parser = new GameCoreParser(this.Root, entities);
        var result = parser.GetGameCores();

        this.ErrorGameCores = parser.ErrorGameCores;

        return result;
    }
}

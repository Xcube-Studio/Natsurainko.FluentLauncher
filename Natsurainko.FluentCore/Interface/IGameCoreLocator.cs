using Natsurainko.FluentCore.Class.Model.Launch;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentCore.Interface;

public interface IGameCoreLocator
{
    DirectoryInfo Root { get; }

    IEnumerable<GameCore> GetGameCores();

    GameCore GetGameCore(string id);
}

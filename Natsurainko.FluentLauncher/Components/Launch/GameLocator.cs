using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Management.Parsing;
using System.IO;

namespace Natsurainko.FluentLauncher.Components.Launch;

internal class GameLocator : DefaultGameLocator
{
    public GameLocator(string folder) : base(folder) { }

    public GameLocator(DirectoryInfo directory) : base(directory.FullName) { }

    protected override string GetName(GameInfo gameInfo, VersionJsonEntity jsonEntity)
    {
        var specialConfig = gameInfo.GetSpecialConfig();
        return string.IsNullOrEmpty(specialConfig.NickName) ? jsonEntity.Id : specialConfig.NickName;
    }
}

using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Management.GameLocator;
using Nrk.FluentCore.Management.Parsing;
using System.IO;

namespace Natsurainko.FluentLauncher.Services.Launch;

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

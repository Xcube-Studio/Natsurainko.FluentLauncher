using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;

namespace Natsurainko.FluentLauncher.Classes.Data.Launch;

internal record ExtendedGameInfo : GameInfo
{
    public DateTime? LastLaunchTime { get; set; }

    //public new ExtendedGameInfo InheritsFrom { get; set; }

    public void InitSpecialConfig()
    {
        var specialConfig = this.GetSpecialConfig();
        LastLaunchTime = specialConfig.LastLaunchTime;
        Name = string.IsNullOrEmpty(specialConfig.NickName) ? AbsoluteId : specialConfig.NickName;
    }

    public GameInfo GetGameInfo() => new()
    {
        AbsoluteId = AbsoluteId,
        AbsoluteVersion = AbsoluteVersion,
        AssetsIndexJsonPath = AssetsIndexJsonPath,
        InheritsFrom = InheritsFrom,// == null ? null : InheritsFrom.GetGameInfo(),
        IsInheritedFrom = IsInheritedFrom,
        IsVanilla = IsVanilla,
        JarPath = JarPath,
        MinecraftFolderPath = MinecraftFolderPath,
        Name = Name,
        Type = Type,
        VersionJsonPath = VersionJsonPath,
    };
}

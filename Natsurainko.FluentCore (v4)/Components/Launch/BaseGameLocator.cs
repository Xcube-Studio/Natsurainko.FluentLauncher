using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Interfaces;
using System;
using System.Collections.Generic;

namespace Nrk.FluentCore.Components.Launch;

/// <summary>
/// 游戏定位器的抽象定义
/// </summary>
public abstract class BaseGameLocator : IGameLocator
{
    public string MinecraftFolderPath { get; set; }

    /// <param name="folder">.minecraft 目录绝对路径</param>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseGameLocator(string folder)
    {
        MinecraftFolderPath = folder ?? throw new ArgumentNullException(nameof(folder));
    }

    public virtual IEnumerable<GameInfo> EnumerateGames() => throw new NotImplementedException();

    public virtual GameInfo GetGame(string absoluteId) => throw new NotImplementedException();

    public virtual IReadOnlyList<GameInfo> GetGames(out IReadOnlyList<string> errorGameNames)
    {
        throw new NotImplementedException();
    }
}

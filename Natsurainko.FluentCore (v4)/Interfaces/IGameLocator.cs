using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Interfaces;

/// <summary>
/// 游戏定位器接口
/// </summary>
public interface IGameLocator
{
    /// <summary>
    /// .minecraft 目录的绝对路径
    /// </summary>
    string MinecraftFolderPath { get; }

    /// <summary>
    /// 根据获取 绝对Id 对应的游戏，如果不存在应返回null
    /// </summary>
    /// <param name="absoluteId">游戏核心的 绝对Id</param>
    /// <returns></returns>
    GameInfo GetGameInfo(string absoluteId);

    /// <summary>
    /// 快速遍历获取游戏核心
    /// </summary>
    /// <returns></returns>
    IEnumerable<GameInfo> EnumerateGames();

    /// <summary>
    /// 获取文件夹下所有游戏核心，并查找出错误的游戏核心
    /// </summary>
    /// <param name="errorGameNames">错误的游戏核心名称（对应 version 文件夹名）</param>
    /// <returns></returns>
    IReadOnlyList<GameInfo> GetGames(out IReadOnlyList<string> errorGameNames);
}

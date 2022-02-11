using FluentCore.Model.Game;
using FluentCore.Model.Launch;
using System.Collections.Generic;

namespace FluentCore.Interface
{
    /// <summary>
    /// 游戏核心定位器
    /// </summary>
    public interface ICoreLocator
    {
        /// <summary>
        /// 游戏根目录
        /// </summary>
        string Root { get; set; }

        /// <summary>
        /// 获取.minecraft目录下的所有游戏核心
        /// </summary>
        /// <returns></returns>
        IEnumerable<GameCore> GetAllGameCores();

        /// <summary>
        /// 获取.minecraft目录下的所有游戏核心的源数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<CoreModel> GetAllCoreModels();

        /// <summary>
        /// 根据id获取对应的核心
        /// </summary>
        /// <param name="id">版本id</param>
        /// <returns></returns>
        GameCore GetGameCoreFromId(string id);

        /// <summary>
        /// 根据id获取对应核心的源数据
        /// </summary>
        /// <param name="id">版本id</param>
        /// <returns></returns>
        CoreModel GetCoreModelFromId(string id);
    }
}

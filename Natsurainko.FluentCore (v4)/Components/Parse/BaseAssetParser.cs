using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using System;
using System.Collections.Generic;

namespace Nrk.FluentCore.Components.Parse;

/// <summary>
/// 依赖材质解析器的抽象定义
/// </summary>
public abstract class BaseAssetParser
{
    protected readonly GameInfo _gameInfo;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameInfo">要解析的游戏核心</param>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseAssetParser(GameInfo gameInfo)
    {
        _gameInfo = gameInfo ?? throw new ArgumentNullException(nameof(gameInfo));
    }

    /// <summary>
    /// 获取 AssetIndexJson 对印的 AssetElement 对象
    /// </summary>
    /// <returns></returns>
    public abstract AssetElement GetAssetIndexJson();

    /// <summary>
    /// 遍历解析依赖材质
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<AssetElement> EnumerateAssets();
}

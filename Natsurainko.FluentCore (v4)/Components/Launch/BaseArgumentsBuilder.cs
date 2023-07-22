using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Interfaces;
using System;
using System.Collections.Generic;

namespace Nrk.FluentCore.Components.Launch;

/// <summary>
/// 参数构建器的抽象定义
/// </summary>
/// <typeparam name="TBuilder"></typeparam>
public abstract class BaseArgumentsBuilder<TBuilder> : IArgumentsBuilder where TBuilder : IArgumentsBuilder
{
    public GameInfo GameInfo { get; init; }

    /// <summary>
    /// 用于构建的核心
    /// </summary>
    /// <param name="gameInfo"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseArgumentsBuilder(GameInfo gameInfo)
    {
        GameInfo = gameInfo ?? throw new ArgumentNullException(nameof(gameInfo));
    }

    /// <summary>
    /// 构建参数
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<string> Build();

    /// <summary>
    /// 设置 Java [必须]
    /// </summary>
    /// <param name="javaPath">Java 可执行文件的绝对路径</param>
    /// <param name="maxMemory">最大虚拟机内存</param>
    /// <param name="minMemory">最小虚拟机内存</param>
    /// <returns></returns>
    public abstract TBuilder SetJavaSettings(string javaPath, int maxMemory, int minMemory);

    /// <summary>
    /// 设置 加载Libraries [必须]
    /// </summary>
    /// <param name="libraryElements"></param>
    /// <returns></returns>
    public abstract TBuilder SetLibraries(IEnumerable<LibraryElement> libraryElements);

    /// <summary>
    /// 设置 账户 [必须]
    /// </summary>
    /// <param name="account">用于游戏的账户</param>
    /// <param name="enableDemoUser">是否启用 Demo 模式</param>
    /// <returns></returns>
    public abstract TBuilder SetAccountSettings(Account account, bool enableDemoUser);

    /// <summary>
    /// 设置 游戏运行目录 [可选]
    /// </summary>
    /// <param name="directory">游戏运行目录</param>
    /// <returns></returns>
    public abstract TBuilder SetGameDirectory(string directory);

    /// <summary>
    /// 设置 游戏额外参数 [可选]
    /// </summary>
    /// <param name="extraVmParameters">额外虚拟机参数</param>
    /// <param name="extraGameParameters">额外游戏参数</param>
    /// <returns></returns>
    public abstract TBuilder AddExtraParameters(IEnumerable<string> extraVmParameters = null, IEnumerable<string> extraGameParameters = null);
}

using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Interfaces;

/// <summary>
/// 参数生成器接口
/// </summary>
public interface IArgumentsBuilder
{
    /// <summary>
    /// 用于生成参数的游戏核心
    /// </summary>
    GameInfo GameInfo { get; }

    IEnumerable<string> Build();
}

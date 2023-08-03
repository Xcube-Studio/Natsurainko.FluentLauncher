using System;

namespace Nrk.FluentCore.Classes.Datas;

/// <summary>
/// 表示 Java 基本信息
/// </summary>
public record JavaInfo
{
    /// <summary>
    /// 版本
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// 发行
    /// </summary>
    public string Company { get; set; }

    public string ProductName { get; set; }

    /// <summary>
    /// 架构体系
    /// </summary>
    public string Architecture { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string Name { get; set; }
}

using Nrk.FluentCore.Classes.Enums;
using System;
using System.Text.RegularExpressions;

namespace Nrk.FluentCore.Classes.Datas.Launch;

/// <summary>
/// 表示游戏日志的一行输出
/// </summary>
public partial record GameLoggerOutput
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public GameLoggerOutputLevel Level { get; private set; }

    /// <summary>
    /// 日志产生线程
    /// </summary>
    public string Thread { get; private set; }

    /// <summary>
    /// 正文内容
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// 源文本
    /// </summary>
    public string FullData { get; private set; }

    /// <summary>
    /// 日志产生时间
    /// </summary>
    public DateTime DateTime { get; private set; }

    /// <summary>
    /// 解析一行日志
    /// </summary>
    /// <param name="data">源文本</param>
    /// <param name="error">是否为错误流输出</param>
    /// <returns></returns>
    public static GameLoggerOutput Parse(string data, bool error = false)
    {
        var timeRegex = TimeRegex().Match(data).Value;
        var regex = LineRegex().Match(data).Value.TrimStart('[').TrimEnd(']');

        var processOutput = new GameLoggerOutput()
        {
            FullData = data,
            Text = data.Contains(": ") ? data[(data.IndexOf(": ") + 2)..] : data,
            DateTime = string.IsNullOrEmpty(timeRegex) ? DateTime.Now : DateTime.Parse(timeRegex),
            Level = GameLoggerOutputLevel.Info,
        };

        if (regex.Contains('/'))
        {
            processOutput.Level = regex.Split('/')[1].ToLower() switch
            {
                "info" => GameLoggerOutputLevel.Info,
                "warn" => GameLoggerOutputLevel.Warn,
                "error" => GameLoggerOutputLevel.Error,
                "datal" => GameLoggerOutputLevel.Fatal,
                "debug" => GameLoggerOutputLevel.Debug,
                _ => GameLoggerOutputLevel.Info,
            };
            processOutput.Thread = regex.Split('/')[0];
        }

        if (data.StartsWith("\tat") || data.Contains(": ") && data.Split(':')[0].EndsWith("Exception"))
            processOutput.Level = GameLoggerOutputLevel.Error;

        if (error)
            processOutput.Level = GameLoggerOutputLevel.Error;

        return processOutput;
    }

    [GeneratedRegex("\\[[\\w/\\s-]+\\]")]
    private static partial Regex LineRegex();

    [GeneratedRegex("([01]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]")]
    private static partial Regex TimeRegex();
}

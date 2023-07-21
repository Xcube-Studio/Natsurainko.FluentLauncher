using Nrk.FluentCore.Classes.Enums;
using System;
using System.Text.RegularExpressions;

namespace Nrk.FluentCore.Classes.Datas.Launch;

public partial record GameLoggerOutput
{
    public GameLoggerOutputLevel Level { get; private set; }

    public string Thread { get; private set; }

    public string Text { get; private set; }

    public string FullData { get; private set; }

    public DateTime DateTime { get; private set; }

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

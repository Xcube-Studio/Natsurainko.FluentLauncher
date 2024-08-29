using ColorCode;
using ColorCode.Styling;
using System.Collections.Generic;

#nullable disable
namespace Natsurainko.FluentLauncher.Utils;

internal class LoggerColorLightLanguage : ILanguage
{
    public static RichTextBlockFormatter Formatter => new(Style);

    public static StyleDictionary Style =>
    [
        new Style("INFO")
        {
            Foreground = "#FFb5cea8",
            ReferenceName = "info"
        },
        new Style("WARN")
        {
            Foreground = "#FFce9178",
            ReferenceName = "warn"
        },
        new Style("Error")
        {
            Foreground = "#FFce9178",
            Bold = true,
            ReferenceName = "error"
        },
        new Style("Debug")
        {
            Foreground = "#FF569cd6",
            ReferenceName = "debug"
        },
        new Style("Date")
        {
            Foreground = "#FF6a9955",
            ReferenceName = "date"
        },
        new Style("Number")
        {
            Foreground = "#FF569cd6",
            ReferenceName = "number"
        },
        new Style("NameSpace")
        {
            Foreground = "#FF569cd6",
            ReferenceName = "namespace"
        },
        new Style("Exception")
        {
            Foreground = "#FFce9178",
            ReferenceName = "exception"
        },
        new Style("AtTrace")
        {
            Foreground = "#FFce9178",
            ReferenceName = "attrace"
        },
    ];

    public string Id => "Logger";

    public string Name => "Logger";

    public string CssClassName => "logger";

    public string FirstLinePattern => null;

    public IList<LanguageRule> Rules => 
    [
        new(@"INFO", new Dictionary<int, string> { { 0, "INFO" } }),
        new(@"WARN", new Dictionary<int, string> { { 0, "WARN" } }),
        new(@"(ERROR|FATAL)", new Dictionary<int, string> { { 0, "Error" } }),
        new(@"DEBUG", new Dictionary<int, string> { { 0, "Debug" } }),
        new(@"(\d{2}:\d{2}:\d{2})", new Dictionary<int, string> { { 0, "Date" } }),
        new(@"\b[0-9]{1,}\b", new Dictionary<int, string> { { 0, "Number" } }),
        new(@"([A-Za-z0-9]+(\.[A-Za-z0-9]+)+)(Exception)", new Dictionary<int, string> { { 0, "Exception" } }),
        new(@"([A-Za-z0-9]+(\.[A-Za-z0-9]+)+)", new Dictionary<int, string> { { 0, "NameSpace" } }),
        new(@"(at )+.*", new Dictionary<int, string> { { 0, "AtTrace" } }),
    ];

    public bool HasAlias(string lang) => lang.ToLower() switch
    {
        "logger" => true,
        _ => false,
    };

    public override string ToString() => Name;
}

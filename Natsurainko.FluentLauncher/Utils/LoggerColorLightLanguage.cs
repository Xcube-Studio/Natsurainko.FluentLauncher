using ColorCode;
using ColorCode.Styling;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Utils;

internal class LoggerColorLightLanguage : ILanguage
{
    public static RichTextBlockFormatter Formatter => new(Style);

    public static StyleDictionary Style => new()
            {
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
            };

    public string Id
    {
        get { return "Logger"; }
    }

    public string Name
    {
        get { return "Logger"; }
    }

    public string CssClassName
    {
        get { return "logger"; }
    }

    public string FirstLinePattern
    {
        get
        {
            return null;
        }
    }

    public IList<LanguageRule> Rules
    {
        get
        {
            return new List<LanguageRule>
                {
                    new LanguageRule(@"INFO",
                        new Dictionary<int, string>
                        {
                            { 0, "INFO" }
                        }),
                    new LanguageRule(@"WARN",
                        new Dictionary<int, string>
                        {
                            { 0, "WARN" }
                        }),
                    new LanguageRule(@"(ERROR|FATAL)",
                        new Dictionary<int, string>
                        {
                            { 0, "Error" }
                        }),
                    new LanguageRule(@"DEBUG",
                        new Dictionary<int, string>
                        {
                            { 0, "Debug" }
                        }),
                    new LanguageRule(@"(\d{2}:\d{2}:\d{2})",
                        new Dictionary<int, string>
                        {
                            { 0, "Date" }
                        }),
                    new LanguageRule(@"\b[0-9]{1,}\b",
                        new Dictionary<int, string>
                        {
                            { 0, "Number" }
                        }),
                    new LanguageRule(@"([A-Za-z0-9]+(\.[A-Za-z0-9]+)+)(Exception)",
                        new Dictionary<int, string>
                        {
                            { 0, "Exception" }
                        }),
                    new LanguageRule(@"([A-Za-z0-9]+(\.[A-Za-z0-9]+)+)",
                        new Dictionary<int, string>
                        {
                            { 0, "NameSpace" }
                        }),
                    new LanguageRule(@"(at )+.*",
                        new Dictionary<int, string>
                        {
                            { 0, "AtTrace" }
                        }),
                };
        }
    }

    public bool HasAlias(string lang)
    {
        switch (lang.ToLower())
        {
            case "logger":
                return true;

            default:
                return false;
        }
    }

    public override string ToString()
    {
        return Name;
    }
}

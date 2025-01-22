using FluentLauncher.Infra.LocalizedStrings;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;

[GeneratedLocalizedStrings]
static partial class LocalizedStrings
{
    public static List<LanguageInfo> SupportedLanguages = [
        new LanguageInfo("en-US", "English"),
        new LanguageInfo("ru-RU", "Русский"),
        new LanguageInfo("uk-UA", "Український"),
        new LanguageInfo("zh-Hans", "简体中文"),
        new LanguageInfo("zh-Hant", "繁體中文")
    ];

    /// <summary>
    /// Get a localized string in Resources.resw
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetString(string key)
    {
        return s_resourceMap.GetValue($"Resources/{key}").ValueAsString;
    }

    public static string[] GetStrings(string key)
    {
        return GetString(key).Split(";");
    }

    public static void ApplyLanguage(string language)
    {
        ApplicationLanguages.PrimaryLanguageOverride = language.Split(',')[0];
        App.GetService<SettingsService>().CurrentLanguage = language;
    }
}

using FluentLauncher.Infra.LocalizedStrings;
using Microsoft.Windows.Globalization;
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
    public static List<string> SupportedLanguages = [
        "en-US, English",
        "ru-RU, Русский",
        "uk-UA, Український",
        "zh-Hans, 简体中文",
        "zh-Hant, 繁體中文"
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

using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Settings;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Utils;

internal static class ResourceUtils
{
    public static List<string> Languages =
    [
        "en-US, English",
        "ru-RU, Русский",
        "uk-UA, Український",
        "zh-Hans, 简体中文",
        "zh-Hant, 繁體中文"
    ];

    public static void ApplyLanguage(string language)
    {
        ApplicationLanguages.PrimaryLanguageOverride = language.Split(',')[0];
        App.GetService<SettingsService>().CurrentLanguage = language;
    }
}

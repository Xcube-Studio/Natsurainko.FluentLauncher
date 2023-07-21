using Microsoft.Windows.AppLifecycle;
using Natsurainko.FluentLauncher.Services.Settings;
using System.Collections.Generic;
using Windows.Globalization;

namespace Natsurainko.FluentLauncher.Components;

// TODO: 更新移动到 Utils
public static class LanguageResources
{
    public static readonly List<string> SupportedLanguages = new()
    {
        "en-US, English",
        "ru-RU, Russian",
        "zh-CN, 简体中文 (中国)",
        "zh-TW, 繁体中文 (中国台湾)"
    };

    public static void ApplyLanguage(string language)
    {
        ApplicationLanguages.PrimaryLanguageOverride = language.Split(',')[0];
        App.GetService<SettingsService>().CurrentLanguage = language;

        AppInstance.Restart(string.Empty);
    }
}

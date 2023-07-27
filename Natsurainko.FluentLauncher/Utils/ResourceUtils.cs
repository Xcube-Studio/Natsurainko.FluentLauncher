using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using Natsurainko.FluentLauncher.Services.Settings;
using System.Collections.Generic;
using Windows.Globalization;

namespace Natsurainko.FluentLauncher.Utils;

internal static class ResourceUtils
{
    private static readonly ResourceManager resourceManager = new();

    public static List<string> Languages = new()
    {
        "en-US, English",
        "ru-RU, Русский",
        "zh-Hans, 简体中文",
        "zh-Hant, 繁體中文"
    };

    public static string GetValue(params string[] strings)
    {
        return resourceManager.MainResourceMap.GetValue($"Resources/{string.Join('_', strings)}").ValueAsString;
    }

    public static string[] GetItems(params string[] strings)
    {
        return resourceManager.MainResourceMap.GetValue($"Resources/{string.Join('_', strings)}").ValueAsString.Split(";");
    }

    public static void ApplyLanguage(string language)
    {
        ApplicationLanguages.PrimaryLanguageOverride = language.Split(',')[0];
        App.GetService<SettingsService>().CurrentLanguage = language;

        AppInstance.Restart(string.Empty);
    }
}

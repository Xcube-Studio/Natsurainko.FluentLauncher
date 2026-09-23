using FluentLauncher.Infra.LocalizedStrings;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Natsurainko.FluentLauncher.Utils;

[GeneratedLocalizedStrings]
static partial class LocalizedStrings
{
    public static List<LanguageInfo> SupportedLanguages = [
        new LanguageInfo("en-US", "English"),
        new LanguageInfo("ru-RU", "Русский"),
        new LanguageInfo("uk-UA", "Українська"),
        new LanguageInfo("zh-Hans", "简体中文"),
        new LanguageInfo("zh-Hant", "繁體中文"),
        new LanguageInfo("pl-PL", "Polski"),
        new LanguageInfo("ja-JP", "日本語"),
        new LanguageInfo("de-DE", "Deutsch"),
        new LanguageInfo("ar-SA", "العربية"),
        new LanguageInfo("fr-FR", "Français"),
        new LanguageInfo("es-ES", "Español"),
        new LanguageInfo("it-IT", "Italiano")
    ];

    /// <summary>
    /// Check whether a given language uses right-to-left script (e.g. Arabic)
    /// </summary>
    public static bool IsRightToLeftLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
            return false;

        string code = language.Split(',')[0].Trim();
        try
        {
            return new CultureInfo(code).TextInfo.IsRightToLeft;
        }
        catch
        {
            return code.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static FlowDirection CurrentFlowDirection =>
        IsRightToLeftLanguage(ApplicationLanguages.PrimaryLanguageOverride)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

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
        string langCode = language.Split(',')[0];
        ApplicationLanguages.PrimaryLanguageOverride = langCode;
        App.GetService<SettingsService>().CurrentLanguage = language;
        ApplyFlowDirection(langCode);
    }

    public static void ApplyFlowDirection(string language)
    {
        bool isRtl = IsRightToLeftLanguage(language);
        var flowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        App.DispatcherQueue?.TryEnqueue(() =>
        {
            if (App.MainWindow?.Content is FrameworkElement mainRoot)
            {
                mainRoot.FlowDirection = flowDirection;
            }
        });
    }
}

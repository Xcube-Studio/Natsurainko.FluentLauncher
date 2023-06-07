using Microsoft.Windows.AppLifecycle;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.Toolkits.Text;
using System.Collections.Generic;
using Windows.Globalization;

namespace Natsurainko.FluentLauncher.Components;

public static class LanguageResources
{
#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
    public static readonly List<string> SupportedLanguages = new()
    {
        "en-US, English"
    };
#else
    public static readonly List<string> SupportedLanguages = new()
    {
        "en-US, English",
        "ru-RU, Russian",
        "zh-CN, 简体中文 (中国)",
        "zh-TW, 繁体中文 (中国台湾)"
    };
#endif

    public static string HandleLaunchState(string message)
    {
        var replaceDictionary = new Dictionary<string, string>()
        {
            { "正在查找游戏核心","Finding Game Core" },
            { "正在补全游戏文件","Completing Game Assets" },
            { "正在验证账户信息","Authenticating Account Information" },
            { "正在构建启动参数","Building Launch Arguments" },
            { "正在启动游戏","Launching the Game" }
        };

        return message.Replace(replaceDictionary);
    }

#if !MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
    public static void ApplyLanguage(string language)
    {
        ApplicationLanguages.PrimaryLanguageOverride = language.Split(',')[0];
        App.GetService<SettingsService>().CurrentLanguage = language;

        AppInstance.Restart(string.Empty);
    }
#endif

}

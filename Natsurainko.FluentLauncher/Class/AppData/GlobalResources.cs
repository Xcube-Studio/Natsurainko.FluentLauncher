using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Module.Mod;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Language = Natsurainko.FluentLauncher.Class.ViewData.Language;

namespace Natsurainko.FluentLauncher.Class.AppData;

internal static class GlobalResources
{
    public static event EventHandler<NavigationViewDisplayMode> NavigationViewDisplayModeChanged;

    public static List<Language> SupportedLanguages => new()
    {
        new() { Code = "zh-CN", DisplayName = "简体中文" },
        new() { Code = "en-US", DisplayName = "English (United States)"},
        new() { Code = "ru-RU", DisplayName = "Русский (Russian)"}
    };

    public static Dictionary<string, KeyValuePair<string, string>[]> OpenJdkDownloadSources => new()
    {
        {
            "OpenJDK 8",
            new KeyValuePair<string, string>[]
            {
                new ("Liberica JDK", "https://download.bell-sw.com/java/8u345+1/bellsoft-jdk8u345+1-windows-amd64-full.zip"),
                new ("jdk.java.net","https://download.java.net/openjdk/jdk8u42/ri/openjdk-8u42-b03-windows-i586-14_jul_2022.zip")
            }
        },
        {
            "OpenJDK 11", new KeyValuePair<string, string>[]
            {
                new ("Liberica JDK", "https://download.bell-sw.com/java/11.0.16.1+1/bellsoft-jdk11.0.16.1+1-windows-amd64.zip"),
                new ("jdk.java.net", "https://download.java.net/openjdk/jdk11/ri/openjdk-11+28_windows-x64_bin.zip"),
                new ("Microsoft", "https://aka.ms/download-jdk/microsoft-jdk-11.0.16-windows-x64.zip")
            }
        },
        {
            "OpenJDK 17", new KeyValuePair<string, string>[]
            {
                new ("Liberica JDK", "https://download.bell-sw.com/java/17.0.4.1+1/bellsoft-jdk17.0.4.1+1-windows-amd64.zip"),
                new ("jdk.java.net", "https://download.java.net/openjdk/jdk17/ri/openjdk-17+35_windows-x64_bin.zip"),
                new ("Microsoft", "https://aka.ms/download-jdk/microsoft-jdk-17.0.4-windows-x64.zip")
            }
        },
        {
            "OpenJDK 18", new KeyValuePair<string, string>[]
            {
                new ("Liberica JDK", "https://download.bell-sw.com/java/18.0.2.1+1/bellsoft-jdk18.0.2.1+1-windows-amd64.zip"),
                new ("jdk.java.net", "https://download.java.net/openjdk/jdk18/ri/openjdk-18+36_windows-x64_bin.zip")
            }
        }
    };

    public static List<string> DownloadSources => new() { "Mojang", "Bmclapi", "Mcbbs" };

    public static ResourceLoader ResourceLoader { get; private set; }

    public static NavigationViewDisplayMode NavigationViewDisplayMode { get; private set; }

    public static CurseForgeModpackFinder CurseForgeModpackFinder { get; private set; } = new(CurseForgeToken);

    public const string ClientId = "0844e754-1d2e-4861-8e2b-18059609badb";

    public const string RedirectUri = "http://localhost:5001/fluentlauncher/auth-response";

    public const string CurseForgeToken = "$2a$10$Awb53b9gSOIJJkdV3Zrgp.CyFP.dI13QKbWn/4UZI4G4ff18WneB6";

    public static void SetResourceLoader(ResourceLoader resourceLoader) => ResourceLoader = resourceLoader;

    public static void SetNavigationViewDisplayMode(NavigationViewDisplayMode navigationViewDisplayMode)
    {
        NavigationViewDisplayMode = navigationViewDisplayMode;
        NavigationViewDisplayModeChanged?.Invoke(null, navigationViewDisplayMode);
    }
}

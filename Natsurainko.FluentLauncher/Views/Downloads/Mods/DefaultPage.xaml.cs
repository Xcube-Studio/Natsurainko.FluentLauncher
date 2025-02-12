using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.Downloads.Mods;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    string IBreadcrumbBarAware.Route => "ModsDownload";

    DefaultViewModel VM => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        this.InitializeComponent();
    }

    private void Page_Unloaded(object _, RoutedEventArgs __) => VM.IsActive = false;

    internal static IEnumerable<string> FilterCategories(IEnumerable<string> categories)
    {
        string[] filteredTags = [.. Enum.GetNames(typeof(ModLoaderType)).Select(x => x.ToLower())];

        foreach (var category in categories)
        {
            if (filteredTags.Contains(category))
                continue;

            yield return category;
        }
    }

    internal static string GetSupportInfo(object mod)
    {
        List<string> supportLoaders = [];

        if (mod is ModrinthResource modrinthResource)
        {
            foreach (var loaderType in Enum.GetValues<ModLoaderType>())
            {
                string loaderName =  loaderType.ToString();
                if (modrinthResource.Categories.Contains(loaderName.ToLower()))
                    supportLoaders.Add(loaderName);
            }

            return string.Join(" | ", string.Join(", ", supportLoaders), GetVersionInfo(modrinthResource.Versions));
        }
        else if (mod is CurseForgeResource curseForgeResource)
        {
            List<string> versions = [];

            foreach (var forgeFile in curseForgeResource.Files)
            {
                string loaderName = forgeFile.ModLoaderType.ToString();

                if (!supportLoaders.Contains(loaderName))
                    supportLoaders.Add(loaderName);

                if (!versions.Contains(forgeFile.McVersion))
                    versions.Add(forgeFile.McVersion);
            }

            return string.Join(" | ", string.Join(", ", supportLoaders), GetVersionInfo(versions));
        }

        return string.Empty;
    }

    internal static string GetLocalizedCategories(string category)
    {
        if (ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans" && ApplicationLanguages.PrimaryLanguageOverride != "zh-Hant")
            return string.Concat(category[0].ToString().ToUpper(), category.AsSpan(1));

        try
        {
            return LocalizedStrings.GetString($"ModCategories__{category
                .Replace(",", string.Empty)
                .Replace("\'", string.Empty)
                .Replace(" ", "_")
                .Replace("&", string.Empty)
                .Replace('-', '_')}");
        }
        catch { }

        return category;
    }

    internal static string GetVersionInfo(IEnumerable<string> versions)
    {
        List<Version> filteredVersions = [.. versions.Where(v => !v.Any(char.IsLetter)).Select(Version.Parse)];
        filteredVersions.Sort();

        var majorVersions = filteredVersions.Select(v => new Version(v.Major, v.Minor)).Distinct().ToList();

        // Find continuous version ranges
        List<string> ranges = [];
        int start = 0;

        while (start < majorVersions.Count)
        {
            int end = start;
            while (end + 1 < majorVersions.Count && majorVersions[end + 1].Minor == majorVersions[end].Minor + 1)
            {
                end++;
            }

            if (end == majorVersions.Count - 1 && majorVersions[end].Minor == 21)
            {
                ranges.Add($"{majorVersions[start].Major}.{majorVersions[start].Minor}+");
            }
            else if (start == end)
            {
                ranges.Add($"{majorVersions[start].Major}.{majorVersions[start].Minor}");
            }
            else
            {
                ranges.Add($"{majorVersions[start].Major}.{majorVersions[start].Minor}-{majorVersions[end].Major}.{majorVersions[end].Minor}");
            }

            start = end + 1;
        }

        return string.Join(", ", ranges);
    }
}

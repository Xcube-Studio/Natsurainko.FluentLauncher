using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal partial class ResourceSupportInfoConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ModrinthResource modrinthResource)
        {
            List<string> supportLoaders = [];

            foreach (var loaderType in Enum.GetValues<ModLoaderType>())
            {
                string loaderName = loaderType.ToString();
                if (modrinthResource.Categories.Contains(loaderName.ToLower()))
                    supportLoaders.Add(loaderName);
            }

            return string.Join(" | ", string.Join(", ", supportLoaders), GetVersionInfo(modrinthResource.Versions));
        }
        else if (value is CurseForgeResource curseForgeResource)
        {
            List<string> versions = [];
            List<string> supportLoaders = [];

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

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static string GetVersionInfo(IEnumerable<string> versions)
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

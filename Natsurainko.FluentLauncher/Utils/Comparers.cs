using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using OptiFineVersion = (string Patch, bool IsPre, int PreVersion);

namespace Natsurainko.FluentLauncher.Utils;

internal class ForgeVersionComparer : IComparer<ForgeInstallData>
{
    public int Compare(ForgeInstallData? x, ForgeInstallData? y)
    {
        if (x == null || y == null)
        {
            if (x == null && y == null)
                return 0;

            if (x == null) return -1;
            if (y == null) return 1;
        }

        if (x.Build != null && y.Build != null)
        {
            if (x.Build == y.Build)
                return 0;
            else if (x.Build > y.Build)
                return 1;
            else return -1;
        }

        int[] versionX = FilterNumbers(x.Version);
        int[] versionY = FilterNumbers(y.Version);

        return Compare(versionX, versionY);
    }

    private static int Compare(int[] x, int[] y)
    {
        if (x.Length != y.Length)
        {
            int compare = x.Length > y.Length
                ? Compare(x.Take(y.Length).ToArray(), y)
                : Compare(x, y.Take(x.Length).ToArray());

            return compare != 0
                ? compare
                : x.Length > y.Length
                    ? 1
                    : -1;
        }

        for (int i = 0; i < x.Length; i++)
            if (x[i] > y[i])
                return 1;
            else if (x[i] < y[i])
                return -1;

        return 0;
    }

    private static int[] FilterNumbers(string text)
    {
        string pattern = @"\d+"; // 匹配一个或多个连续的数字

        IEnumerable<int> GetValues()
        {
            var matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
                yield return int.Parse(match.Value);
        }

        return GetValues().ToArray();
    }
}

public partial class OptiFineVersionComparer : IComparer<OptiFineInstallData>
{
    public int Compare(OptiFineInstallData? x, OptiFineInstallData? y)
    {
        if (x == null || y == null)
        {
            if (x == null && y == null)
                return 0;

            if (x == null) return -1;
            if (y == null) return 1;
        }

        var versionX = ParseOptiFineVersion(x);
        var versionY = ParseOptiFineVersion(y);

        if (versionX.Patch != versionY.Patch)
            return string.Compare(versionX.Patch, versionY.Patch);
        else
        {
            if (!versionX.IsPre && versionY.IsPre)
                return 1;
            else if (versionX.IsPre && !versionY.IsPre)
                return -1;
            else return versionX.PreVersion.CompareTo(versionY.PreVersion);
        }
    }

    private static OptiFineVersion ParseOptiFineVersion(OptiFineInstallData optiFineInstallData)
    {
        var optiFineVersion = new OptiFineVersion();

        Regex regex = ParseOptiFineVersionRegex();
        Match match = regex.Match($"{optiFineInstallData.Type}_{optiFineInstallData.Patch}");

        if (match.Success)
        {
            optiFineVersion.Patch = match.Groups[1].Value;

            if (match.Groups[3].Success)
            {
                optiFineVersion.IsPre = true;
                optiFineVersion.PreVersion = int.Parse(match.Groups[3].Value);
            }
        }
        else throw new InvalidOperationException();

        return optiFineVersion;
    }

    [GeneratedRegex(@"^HD_U_([A-Z][0-9])(_pre(\d+))?$")]
    private static partial Regex ParseOptiFineVersionRegex();
}

internal class StringComparer
{
    public static int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }
        return d[s.Length, t.Length];
    }
}
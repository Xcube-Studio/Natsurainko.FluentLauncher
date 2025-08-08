using System;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class LongExtensions
{
    public static string ToFileSizeString(long size)
    {
        if (size < 0)
            return "0 B";

        double d = size;
        int i = 0;

        while ((d > 1024) && (i < 5))
        {
            d /= 1024;
            i++;
        }

        var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
        return string.Format("{0} {1}", Math.Round(d, 2), unit[i]);
    }
}

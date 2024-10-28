using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class StringExtensions
{
    public static int[] FilterNumbers(this string text)
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

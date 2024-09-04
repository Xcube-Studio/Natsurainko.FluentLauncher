using System.Text.RegularExpressions;

namespace Natsurainko.FluentLauncher.Utils;

internal static partial class PathUtils
{
    private static readonly Regex CheckRegex = GenerateCheckRegex();

    [GeneratedRegex(@"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()~!@#$%^&()\[\]{}+=]+\\?)*$")]
    private static partial Regex GenerateCheckRegex();

    public static bool IsValidPath(string path) => CheckRegex.IsMatch(path);
}

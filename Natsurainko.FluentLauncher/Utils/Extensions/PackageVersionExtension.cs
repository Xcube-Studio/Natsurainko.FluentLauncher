using Windows.ApplicationModel;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class PackageVersionExtension
{
    public static string GetVersionString(this PackageVersion packageVersion)
    {
        return string.Format("{0}.{1}.{2}.{3}",
            packageVersion.Major,
            packageVersion.Minor,
            packageVersion.Build,
            packageVersion.Revision);
    }
}

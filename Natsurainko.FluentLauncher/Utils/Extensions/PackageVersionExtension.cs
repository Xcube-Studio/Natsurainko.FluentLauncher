using Windows.ApplicationModel;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class PackageVersionExtension
{
    public static string GetVersionString(this PackageVersion packageVersion)
    {
        return string.Format("{0}.{1}.{2}.{3}",
            Package.Current.Id.Version.Major,
            Package.Current.Id.Version.Minor,
            Package.Current.Id.Version.Build,
            Package.Current.Id.Version.Revision);
    }
}

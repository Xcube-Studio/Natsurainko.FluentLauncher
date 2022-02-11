using System.Runtime.InteropServices;

namespace FluentCore.Service.Local
{
    public class PathHelper
    {
        public static readonly string X = SystemConfiguration.Platform == OSPlatform.Windows ? "\\" : "/";

        public static string GetVersionsFolder(string root) => $"{root}{X}versions";

        public static string GetVersionFolder(string root, string id) => $"{root}{X}versions{X}{id}";

        public static string GetLibrariesFolder(string root) => $"{root}{X}libraries";

        public static string GetAssetsFolder(string root) => $"{root}{X}assets";

        public static string GetAssetIndexFolder(string root) => $"{root}{X}assets{X}indexes";

        public static string GetLogConfigsFolder(string root) => $"{GetAssetsFolder(root)}{X}log_configs";
    }
}

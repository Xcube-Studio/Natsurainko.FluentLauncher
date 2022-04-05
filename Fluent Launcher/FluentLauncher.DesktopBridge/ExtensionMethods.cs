using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.DesktopBridge
{
    public static class ExtensionMethods
    {
        public static string EncodeBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        public static string DecodeBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var valueBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public static string LengthToMb(this long value) => $"{value / (1024.0 * 1024.0):0.0} Mb";
    }
}

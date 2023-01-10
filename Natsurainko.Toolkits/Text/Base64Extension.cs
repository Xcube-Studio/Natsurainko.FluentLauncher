using System;
using System.Text;

namespace Natsurainko.Toolkits.Text
{
    public static class Base64Extension
    {
        public static string ConvertToBase64(this string value)
        {
            try { return string.IsNullOrEmpty(value) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(value)); }
            catch (Exception ex) { return $"{ex.Message}\r\n{ex.StackTrace}"; }
        }

        public static string ConvertToString(this string value)
        {
            try { return string.IsNullOrEmpty(value) ? string.Empty : Encoding.UTF8.GetString(Convert.FromBase64String(value)); }
            catch (Exception ex) { return $"{ex.Message}\r\n{ex.StackTrace}"; }
        }
    }
}

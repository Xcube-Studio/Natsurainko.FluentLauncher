using System;
using System.Text.RegularExpressions;

namespace Natsurainko.Toolkits.Text
{
    public static class UriExtension
    {
        public static string GetParameterUrl(this Uri uri, string paraName, bool isDecode = false)
        {
            var regex = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);

            foreach (Match match in regex.Matches(uri.OriginalString))
            {
                if (match.Result("$2").Equals(paraName))
                {
                    string paraResult = match.Result("$3");
                    if (isDecode)
                        return System.Web.HttpUtility.UrlDecode(paraResult);
                    else
                        return paraResult;
                }
            }

            return null;
        }
    }
}

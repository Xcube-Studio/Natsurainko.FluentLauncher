using System;
using System.Security.Cryptography;
using System.Text;

namespace Natsurainko.Toolkits.Values
{
    public static class GuidHelper
    {
        public static Guid FromString(string value)
        {
            using var md5 = MD5.Create();
            return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}

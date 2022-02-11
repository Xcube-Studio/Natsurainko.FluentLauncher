using System;
using System.Security.Cryptography;
using System.Text;

namespace FluentCore.Service.Local
{
    public class UuidHelper
    {
        public static Guid FromString(string input)
        {
            using var md5 = MD5.Create();
            return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
    }
}

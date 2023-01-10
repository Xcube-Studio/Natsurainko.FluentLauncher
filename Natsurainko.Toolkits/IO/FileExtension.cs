using System;
using System.IO;
using System.Security.Cryptography;

namespace Natsurainko.Toolkits.IO
{
    public static class FileExtension
    {
        public static bool Verify(this FileInfo file, int size)
            => file.Exists ? file.Length == size : false;

        public static bool Verify(this FileInfo file, string sha1)
        {
            if (!file.Exists)
                return false;

            try
            {
                using var fileStream = File.OpenRead(file.FullName);
                using var provider = new SHA1CryptoServiceProvider();
                byte[] bytes = provider.ComputeHash(fileStream);

                return sha1.ToLower() == BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
            catch
            {
                return false;
            }
        }
    }
}

using FluentCore.Model;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace FluentCore.Service.Local
{
    public class FileHelper
    {
        public static bool FileVerify(FileInfo file, int? size = null, string sha1 = default, string MD5 = default)
        {
            if (!file.Exists)
                throw new FileNotFoundException(file.FullName);

            bool sizeSame = true, sha1Same = true, MD5Same = true;

            if (size.HasValue)
                sizeSame = file.Length == size;
            if (!string.IsNullOrEmpty(sha1))
                sha1Same = GetSha1(file).ToLower() == sha1.ToLower();
            if (!string.IsNullOrEmpty(MD5))
                MD5Same = GetMD5(file).ToLower() == MD5.ToLower();

            return sizeSame && sha1Same && MD5Same;
        }

        public static bool FileVerifyHttpDownload(HttpDownloadRequest request, HttpDownloadResponse response) => FileVerify(response.FileInfo, request.Size, request.Sha1);

        public static string GetMD5(FileInfo file)
        {
            using var fileStream = File.OpenRead(file.FullName);
            using var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(fileStream);

            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string GetSha1(FileInfo file)
        {
            using var fileStream = File.OpenRead(file.FullName);
            using var sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes = sha1.ComputeHash(fileStream);

            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static void DeleteAllFiles(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
                file.Delete();

            directory.GetDirectories().ToList().ForEach(x =>
            {
                DeleteAllFiles(x);
                x.Delete();
            });

        }

        public static string FindFile(DirectoryInfo directory,string filename)
        {
            foreach (var items in directory.GetFiles())
                if (items.Name == filename)
                    return items.FullName;

            foreach (var items in directory.GetDirectories())
                return FindFile(items, filename);

            return null;
        }
    }
}

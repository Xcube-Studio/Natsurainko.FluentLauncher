using Nrk.FluentCore.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Nrk.FluentCore.Utils;

public static class IDownloadElementExtensions
{
    /// <summary>
    /// 验证文件
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static bool VerifyFile(this IDownloadElement element)
    {
        if (!File.Exists(element.AbsolutePath))
            return false;

        if (!string.IsNullOrEmpty(element.Checksum))
        {
            using var fileStream = File.OpenRead(element.AbsolutePath);

            return BitConverter.ToString(SHA1.HashData(fileStream)).Replace("-", string.Empty)
                .ToLower().Equals(element.Checksum);
        }

        return true;
    }
}

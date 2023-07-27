using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Utils;

public static class StringExtensions
{
    /// <summary>
    /// 若字符串中含有空格，则在原文本头尾加上 "
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ToPathParameter(this string text) => text.Contains(' ') ? "\"" + text + "\"" : text;

    /// <summary>
    /// 依照字典的值和键替换文本
    /// </summary>
    /// <param name="text"></param>
    /// <param name="keyValuePairs"></param>
    /// <returns></returns>
    public static string ReplaceFromDictionary(this string text, Dictionary<string,string> keyValuePairs)
    {
        string replacedText = text;

        foreach (var item in keyValuePairs)
            replacedText = replacedText.Replace(item.Key, item.Value);

        return replacedText;
    }

    public static IEnumerable<string> FormatLibraryName(string Name)
    {
        var extension = Name.Contains('@') ? Name.Split('@') : Array.Empty<string>();
        var subString = extension.Any()
            ? Name.Replace($"@{extension[1]}", string.Empty).Split(':')
            : Name.Split(':');

        foreach (string item in subString[0].Split('.'))
            yield return item;

        yield return subString[1];
        yield return subString[2];

        if (!extension.Any())
            yield return $"{subString[1]}-{subString[2]}{(subString.Length > 3 ? $"-{subString[3]}" : string.Empty)}.jar";
        else yield return $"{subString[1]}-{subString[2]}{(subString.Length > 3 ? $"-{subString[3]}" : string.Empty)}.jar".Replace("jar", extension[1]);
    }

    public static string FormatLibraryNameToRelativePath(string name)
    {
        string path = string.Empty;

        foreach (var subPath in FormatLibraryName(name))
                path = Path.Combine(path, subPath);

        return path;
    }

    /// <summary>
    /// 参数组
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static IEnumerable<string> ArgumnetsGroup(IEnumerable<string> parameters)
    {
        var parameter = new List<string>();

        foreach (var item in parameters)
        {
            if (parameter.Any() && parameter[0].StartsWith("-") && item.StartsWith("-"))
            {
                yield return parameter[0].Trim(' ');

                parameter = new List<string> { item };
            }
            else if (parameters.Last() == item && !parameter.Any())
                yield return item.Trim(' ');
            else parameter.Add(item);

            if (parameter.Count == 2)
            {
                yield return string.Join(" ", parameter).Trim(' ');
                parameter = new List<string>();
            }
        }
    }

    public static string ConvertFromBase64(this string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));

    public static void ParseServerAddress(this string value, out string host, out int? port)
    {
        if (value.Contains(':'))
        {
            var strs = value.Split(':');
            host = strs[0];
            port = int.Parse(strs[1]);

            return;
        }

        host = value;
        port = 25565;
    }
}

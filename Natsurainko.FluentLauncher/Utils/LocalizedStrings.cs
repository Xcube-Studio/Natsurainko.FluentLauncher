using FluentLauncher.Infra.LocalizedStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;

[GeneratedLocalizedStrings]
static partial class LocalizedStrings
{
    /// <summary>
    /// Get a localized string in Resources.resw
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetString(string key)
    {
        return s_resourceMap.GetValue($"Resources/{key}").ValueAsString;
    }

    public static string[] GetStrings(string key)
    {
        return GetString(key).Split(";");
    }
}

using Tomlyn.Model;

namespace Nrk.FluentCore.Utils;

public static class TomlExtensions
{
    public static string GetString(this TomlTable table, string key)
        => !table.ContainsKey(key) ? null : table[key].ToString();
}

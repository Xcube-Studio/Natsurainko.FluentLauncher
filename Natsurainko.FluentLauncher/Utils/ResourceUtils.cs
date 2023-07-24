using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;

internal static class ResourceUtils
{
    private static readonly ResourceManager resourceManager = new();

    public static string GetValue(params string[] strings)
    {
        return resourceManager.MainResourceMap.GetValue($"Resources/{string.Join('_', strings)}").ValueAsString;
    }
}

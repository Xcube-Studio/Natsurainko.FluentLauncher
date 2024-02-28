using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;

internal static class NavigationViewItemExtensions
{
    public static string GetTag(this NavigationViewItem item)
    {
        return item.Tag?.ToString()
            ?? throw new ArgumentNullException("The item's tag is null.");
    }
}

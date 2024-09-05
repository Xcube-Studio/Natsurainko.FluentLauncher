using Microsoft.UI.Xaml.Controls;
using System;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class ControlExtensions
{
    public static string GetTag(this Control item)
    {
        return item.Tag?.ToString()
            ?? throw new ArgumentNullException(nameof(item.Tag), "The control's tag is null.");
    }
}

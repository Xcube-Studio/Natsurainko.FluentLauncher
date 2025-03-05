using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class ControlExtensions
{
    public static string? GetTag(this Control item) => item.Tag?.ToString();
}

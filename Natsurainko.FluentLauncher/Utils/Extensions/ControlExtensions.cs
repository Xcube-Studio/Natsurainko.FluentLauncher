using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class ControlExtensions
{
    public static string? GetTag(this Control item) => item.Tag?.ToString();

    public static TElement? FindChild<TElement>(this DependencyObject dependencyObject, string name)
        where TElement : FrameworkElement
    {
        if (dependencyObject is null) return null;
        if (dependencyObject is TElement element && element.Name == name) return element;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
        {
            if (VisualTreeHelper.GetChild(dependencyObject, i) is not UIElement child)
                continue;

            if (child.FindChild<TElement>(name) is TElement result)
                return result;
        }

        return null;
    }
}

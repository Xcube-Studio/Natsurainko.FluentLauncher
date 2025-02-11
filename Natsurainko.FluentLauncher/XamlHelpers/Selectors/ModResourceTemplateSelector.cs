using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nrk.FluentCore.Resources;

namespace Natsurainko.FluentLauncher.XamlHelpers.Selectors;

#nullable disable
internal class ModResourceTemplateSelector : DataTemplateSelector
{
    public DataTemplate CurseForgeTemplate { get; set; }

    public DataTemplate ModrinthTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is CurseForgeResource)
            return CurseForgeTemplate;
        else if (item is ModrinthResource)
            return ModrinthTemplate;

        return base.SelectTemplateCore(item);
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}

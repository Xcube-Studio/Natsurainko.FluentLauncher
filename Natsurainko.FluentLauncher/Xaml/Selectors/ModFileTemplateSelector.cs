using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nrk.FluentCore.Resources;

namespace Natsurainko.FluentLauncher.Xaml.Selectors;

#nullable disable
internal class ModFileTemplateSelector : DataTemplateSelector
{
    public DataTemplate CurseForgeTemplate { get; set; }

    public DataTemplate ModrinthTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is CurseForgeFile)
            return CurseForgeTemplate;
        else if (item is ModrinthFile)
            return ModrinthTemplate;

        return base.SelectTemplateCore(item);
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}

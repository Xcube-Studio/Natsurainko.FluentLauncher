using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nrk.FluentCore.Classes.Datas.Download;

namespace Natsurainko.FluentLauncher.Utils.Xaml.TemplateSelectors;

internal class ResourceTemplateSelector : DataTemplateSelector
{
    public DataTemplate CurseResource { get; set; }

    public DataTemplate ModrinthResource { get; set; }

    public DataTemplate MinecraftCore { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is CurseResource)
            return CurseResource;

        if (item is VersionManifestItem)
            return MinecraftCore;

        return base.SelectTemplateCore(item);
    }
}

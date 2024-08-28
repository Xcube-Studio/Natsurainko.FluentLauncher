using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Selectors;

internal class ResourceItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate Modrinth { get; set; }

    public DataTemplate CurseForge { get; set; }

    public DataTemplate Minecraft { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is CurseForgeResource)
            return CurseForge;
        else if (item is ModrinthResource)
            return Modrinth;
        if (item is VersionManifestItem)
            return Minecraft;

        return base.SelectTemplateCore(item);
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}

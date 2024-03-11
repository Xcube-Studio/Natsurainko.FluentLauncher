using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.UI;

namespace Natsurainko.FluentLauncher.Utils.Xaml.TemplateSelectors;

internal class DownloadProcessSelector : DataTemplateSelector
{
    public DataTemplate ResourceDownload { get; set; }

    public DataTemplate CoreInstall { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is ResourceDownloadProcess)
            return ResourceDownload;

        if (item is CoreInstallProcess)
            return CoreInstall;

        return base.SelectTemplateCore(item);
    }
}

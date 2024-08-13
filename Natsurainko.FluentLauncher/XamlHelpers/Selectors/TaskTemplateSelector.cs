using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Selectors;

internal class TaskTemplateSelector : DataTemplateSelector
{
    public DataTemplate LaunchSession { get; set; }

    public DataTemplate FileDownloadProcess { get; set; }

    public DataTemplate InstallProcess { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is LaunchSessionViewModel)
            return LaunchSession;
        else if (item is FileDownloadProcessViewModel)
            return FileDownloadProcess;
        else if (item is InstallProcessViewModel)
            return InstallProcess;

        return base.SelectTemplateCore(item);
    }
}

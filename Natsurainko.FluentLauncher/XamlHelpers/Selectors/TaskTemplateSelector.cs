using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Selectors;

internal class TaskTemplateSelector : DataTemplateSelector
{
    public DataTemplate LaunchTask { get; set; }

    public DataTemplate DownloadGameResourceTask { get; set; }

    public DataTemplate InstallInstanceTask { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is LaunchTaskViewModel)
            return LaunchTask;
        else if (item is DownloadGameResourceTaskViewModel)
            return DownloadGameResourceTask;
        else if (item is InstallInstanceTaskViewModel)
            return InstallInstanceTask;

        return base.SelectTemplateCore(item);
    }
}

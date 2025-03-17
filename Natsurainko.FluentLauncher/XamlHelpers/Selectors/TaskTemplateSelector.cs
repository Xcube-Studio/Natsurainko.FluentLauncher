using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Selectors;

internal partial class TaskTemplateSelector : DataTemplateSelector
{
    public DataTemplate LaunchTask { get; set; }

    public DataTemplate DownloadModTask { get; set; }

    public DataTemplate InstallInstanceTask { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is LaunchTaskViewModel)
            return LaunchTask;
        else if (item is DownloadModTaskViewModel)
            return DownloadModTask;
        else if (item is InstallInstanceTaskViewModel)
            return InstallInstanceTask;

        return base.SelectTemplateCore(item);
    }
}

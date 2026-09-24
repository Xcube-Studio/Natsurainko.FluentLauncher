using CommunityToolkit.WinUI.Controls;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Instances;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Views.Instances;

public sealed partial class ResourcePackPage : Page, IBreadcrumbBarAware
{
    public string Route => "ResourcePack";

    internal ResourcePackViewModel VM => (ResourcePackViewModel)DataContext;

    public ResourcePackPage()
    {
        InitializeComponent();
    }

    private void Page_DragEnter(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void Page_Drop(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var paths = new System.Collections.Generic.List<string>();

            foreach (var item in items)
                paths.Add(item.Path);

            await VM.AddResourcePacksAsync(paths.ToArray());
        }
    }

    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle && toggle.DataContext is ResourcePackItem item)
        {
            if (item.IsEnabled != toggle.IsOn)
            {
                if (!item.Switch(toggle.IsOn))
                {
                    toggle.IsOn = item.IsEnabled;
                }
            }
        }
    }

    private void MenuFlyout_Opened(object sender, object e)
    {
        if (sender is not MenuFlyout menuFlyout) return;

        foreach (var item in menuFlyout.Items)
        {
            if (item is MenuFlyoutItem menuItem)
            {
                menuItem.Command = (menuItem.Tag as string) switch
                {
                    "Open" => VM.OpenResourcePackCommand,
                    "CheckUpdate" => VM.CheckUpdateCommand,
                    "Delete" => VM.ConfirmDeleteCommand,
                    _ => menuItem.Command
                };
            }
        }
    }
}

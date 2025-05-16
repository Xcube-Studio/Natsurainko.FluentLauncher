using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Instances;
using Nrk.FluentCore.GameManagement.Mods;
using System;
using System.IO;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Views.Instances;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    public string Route => "Mod";

    ModViewModel VM => (ModViewModel)DataContext;

    public ModPage()
    {
        InitializeComponent();
    }

    private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
    {
        void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            var modInfo = (MinecraftMod)toggleSwitch.DataContext;
            var modsManager = ((ModViewModel)DataContext).ModsManager;

            if (modInfo != null)
                modsManager.Switch(modInfo, toggleSwitch.IsOn);
        }

        var toggleSwitch = (ToggleSwitch)sender;

        toggleSwitch.Toggled += ToggleSwitch_Toggled;
        toggleSwitch.Unloaded += (_, _) => toggleSwitch.Toggled -= ToggleSwitch_Toggled;
    }

    private void Page_DragEnter(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        App.GetService<INotificationService>().ModDrag();
    }

    private async void Page_Drop(object sender, DragEventArgs e)
    {
        int modCount = 0;
        e.AcceptedOperation = DataPackageOperation.Copy;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            foreach (var item in await e.DataView.GetStorageItemsAsync())
            {
                FileInfo fileInfo = new(item.Path);

                if (fileInfo.Extension != ".jar")
                    continue;

                fileInfo.CopyTo(Path.Combine(VM.ModsFolder, fileInfo.Name), true);
                modCount++;
            }
        }

        if (modCount > 0)
        {
            App.GetService<INotificationService>().ModAdded(modCount);
            VM.LoadModList();
        }
    }
}

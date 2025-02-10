using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;
using Nrk.FluentCore.GameManagement.Installer;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.Downloads.Instances;

public sealed partial class InstallPage : Page, IBreadcrumbBarAware
{
    string IBreadcrumbBarAware.Route => "Install";

    InstallViewModel VM => (InstallViewModel)DataContext;

    public InstallPage()
    {
        this.InitializeComponent();
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        List<InstanceLoaderItem> instanceLoaders = [.. sender.SelectedItems.Cast<InstanceLoaderItem>()];
        VM.InstanceLoaderItems = instanceLoaders;

        WeakReferenceMessenger.Default.Send(new InstanceLoaderSelectedMessage(instanceLoaders));
    }

    private void ItemsView_SelectionChanged_1(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        List<InstanceModItem> instanceMods = [.. sender.SelectedItems.Cast<InstanceModItem>()];
        VM.InstanceModItems = instanceMods;
    }

    internal static string GetLoaderVersionFromInstallData(object installData)
    {
        if (installData is ForgeInstallData forgeInstallData)
            return $"{forgeInstallData.Version}{(string.IsNullOrEmpty(forgeInstallData.Branch) ? string.Empty : $"-{forgeInstallData.Branch}")}";
        else if (installData is OptiFineInstallData optiFineInstallData)
            return $"{optiFineInstallData.Type}_{optiFineInstallData.Patch}";
        else if (installData is FabricInstallData fabricInstallData)
            return $"{fabricInstallData.Loader.Version}";
        else if (installData is QuiltInstallData quiltInstallData)
            return $"{quiltInstallData.Loader.Version}";

        return string.Empty;
    }
}

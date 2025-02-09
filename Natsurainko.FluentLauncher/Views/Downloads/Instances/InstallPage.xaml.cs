using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;
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
        WeakReferenceMessenger.Default.Send(new InstanceLoaderSelectedMessage([.. sender.SelectedItems.Cast<InstanceLoaderItem>()]));
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Install;
using Natsurainko.FluentCore.Module.Installer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Models;

public partial class ModLoader : ObservableObject
{
    public ModLoader(ModLoaderType type, string mcVerion)
    {
        Type = type;
        McVersion = mcVerion;

        Task.Run(async () =>
        {
            IEnumerable<IModLoaderInstallBuild> builds = type switch
            {
                ModLoaderType.Forge => await MinecraftForgeInstaller.GetForgeBuildsFromMcVersionAsync(mcVerion),
                ModLoaderType.OptiFine => await MinecraftOptiFineInstaller.GetOptiFineBuildsFromMcVersionAsync(mcVerion),
                ModLoaderType.Fabric => await MinecraftFabricInstaller.GetFabricBuildsFromMcVersionAsync(mcVerion),
                ModLoaderType.Quilt => await MinecraftQuiltInstaller.GetQuiltBuildsFromMcVersionAsync(mcVerion),
                _ => null,
            };

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Builds = builds.ToList();
                SelectedBuild = builds.Any()
                    ? Builds[0] : null;
                IsEnable = builds.Any();

                UnsupportedVisibility = builds.Any()
                    ? Visibility.Collapsed : Visibility.Visible;
                SupportedVisibility = builds.Any()
                    ? Visibility.Visible : Visibility.Collapsed;
            });
        });
    }

    private ListViewItem Item;

    [ObservableProperty]
    private string mcVersion;

    [ObservableProperty]
    private ModLoaderType type;

    [ObservableProperty]
    private bool isSelected = false;

    [ObservableProperty]
    private bool isEnable = false;

    [ObservableProperty]
    private Visibility comboBoxVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility unsupportedVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility supportedVisibility = Visibility.Visible;

    [ObservableProperty]
    private List<IModLoaderInstallBuild> builds;

    [ObservableProperty]
    private IModLoaderInstallBuild selectedBuild;

    [RelayCommand]
    public void Loaded(object parameter)
    {
        Item = parameter as ListViewItem;

        BindingOperations.SetBinding(Item, ListViewItem.IsSelectedProperty, new Binding()
        {
            Mode = BindingMode.TwoWay,
            Source = this,
            Path = new PropertyPath(nameof(IsSelected))
        });

        BindingOperations.SetBinding(Item, ListViewItem.IsEnabledProperty, new Binding()
        {
            Mode = BindingMode.TwoWay,
            Source = this,
            Path = new PropertyPath(nameof(IsEnable))
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsSelected))
            ComboBoxVisibility = IsSelected
                ? Visibility.Visible
                : Visibility.Collapsed;

    }
}

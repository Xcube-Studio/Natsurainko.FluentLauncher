using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Model.Install;
using Natsurainko.FluentCore.Model.Install.Vanilla;
using Natsurainko.FluentCore.Module.Installer;
using Natsurainko.FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Installations;

public partial class Core : ObservableObject
{
    public Core() 
    {
        LoadCores();
    }

    private Dictionary<ModLoaderType, string[]> SupportedVersion;

    private static CoreManifest CoreManifest;

    [ObservableProperty]
    private ObservableCollection<CoreManifestItem> cores;

    [ObservableProperty]
    private CoreManifestItem selectedCoreManifestItem;

    [ObservableProperty]
    private ObservableCollection<ModLoader> loaders;

    [ObservableProperty]
    private ModLoader selectedModLoader;

    [ObservableProperty]
    private string filter = "Release";

    [ObservableProperty]
    private Visibility buildVisibility = Visibility.Collapsed;

    public ObservableCollection<CoreManifestItem> GetFilteredCores()
    {
        IEnumerable<CoreManifestItem> filtered = default;

        if (Filter.Equals("Release"))
            filtered = CoreManifest.Cores.Where(x => x.Type.Equals("release"));
        else if (Filter.Equals("Snapshot"))
            filtered = CoreManifest.Cores.Where(x => x.Type.Equals("snapshot"));
        else if (Filter.Equals("Old"))
            filtered = CoreManifest.Cores.Where(x => x.Type.Contains("old_"));

        return new ObservableCollection<CoreManifestItem>(filtered);
    }

    public void LoadCores()
    {
        Task.Run(async () =>
        {
            if (CoreManifest == null)
                CoreManifest = await MinecraftVanlliaInstaller.GetCoreManifestAsync();

            var filtered = GetFilteredCores();

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Cores = filtered;
                SelectedCoreManifestItem = filtered[0];
            });
        });
    }

    public void LoadLoaders()
    {
        Task.Run(async () =>
        {
            var loaders = new ObservableCollection<ModLoader>();

            if (SupportedVersion == null)
            {
                SupportedVersion = new();

                SupportedVersion.Add(ModLoaderType.Forge, await MinecraftForgeInstaller.GetSupportedMcVersionsAsync());
                SupportedVersion.Add(ModLoaderType.Fabric, await MinecraftFabricInstaller.GetSupportedMcVersionsAsync());
                SupportedVersion.Add(ModLoaderType.OptiFine, await MinecraftOptiFineInstaller.GetSupportedMcVersionsAsync());
            }

            App.MainWindow.DispatcherQueue.TryEnqueue(() => Loaders = loaders);

            if (SupportedVersion != null)
                foreach (var kvp in SupportedVersion)
                {
                    var modloader = new ModLoader(kvp.Key, SelectedCoreManifestItem?.Id)
                    {
                        IsEnable = kvp.Value.Contains(SelectedCoreManifestItem?.Id),
                    };

                    App.MainWindow.DispatcherQueue.TryEnqueue(() => loaders.Add(modloader));
                }
        });
    }

    [RelayCommand]
    public void RemoveLoader() => SelectedModLoader = null;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Filter))
            LoadCores();

        if (e.PropertyName == nameof(SelectedCoreManifestItem))
            LoadLoaders();

        if (e.PropertyName == nameof(SelectedModLoader))
            BuildVisibility = SelectedModLoader?.SelectedBuild == null
                ? Visibility.Collapsed 
                : Visibility.Visible;
    }
}

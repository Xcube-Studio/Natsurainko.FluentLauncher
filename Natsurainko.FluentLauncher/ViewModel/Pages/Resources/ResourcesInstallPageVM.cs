using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Install.Vanilla;
using Natsurainko.FluentCore.Module.Installer;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Resources;

public class ResourcesInstallPageVM : ViewModelBase<Page>
{
    public ResourcesInstallPageVM(Page control) : base(control)
    {
        DispatcherHelper.RunAsync(async () =>
        {
            CoreManifestItems = await GetCoreManifestItems();
            CoreManifestItem = CoreManifestItems.First();
        });
    }

    [Reactive]
    public List<CoreManifestItemViewData> CoreManifestItems { get; set; }

    [Reactive]
    public CoreManifestItemViewData CoreManifestItem { get; set; }

    [Reactive]
    public bool EnableLoader { get; set; }

    [Reactive]
    public bool CanEnableLoader { get; set; }

    [Reactive]
    public bool CanStart { get; set; }

    [Reactive]
    public bool ShowSnapshotEdition { get; set; }

    [Reactive]
    public bool ShowOldEdition { get; set; }

    [Reactive]
    public List<string> SupportedLoaderTypes { get; set; }

    [Reactive]
    public string SelectedLoaderType { get; set; }

    [Reactive]
    public Visibility CanEnableLoaderTips { get; set; }

    [Reactive]
    public Visibility EnableLoaderExpander { get; set; }

    [Reactive]
    public Visibility ModLoaderListLoading { get; set; } = Visibility.Collapsed;

    [Reactive]
    public List<ModLoaderInformationViewData> ModLoaderInformations { get; set; }

    [Reactive]
    public ModLoaderInformationViewData SelectedModLoader { get; set; }

    public override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CoreManifestItem) && CoreManifestItem != null)
        {
            var loaders = new List<string>();

            if ((CacheResources.FabricSupportedMcVersions?.Contains(CoreManifestItem.Data.Id)).GetValueOrDefault())
                loaders.Add("Fabric");

            if ((CacheResources.ForgeSupportedMcVersions?.Contains(CoreManifestItem.Data.Id)).GetValueOrDefault())
                loaders.Add("Forge");

            if ((CacheResources.OptiFineSupportedMcVersions?.Contains(CoreManifestItem.Data.Id)).GetValueOrDefault())
                loaders.Add("OptiFine");

            CanEnableLoader = loaders.Any();

            if (!CanEnableLoader)
                EnableLoader = false;

            SupportedLoaderTypes = loaders;
        }

        if (e.PropertyName == nameof(SelectedLoaderType))
        {
            ModLoaderInformations = null;
            ModLoaderListLoading = Visibility.Visible;

            DispatcherHelper.RunAsync(async () =>
            {
                switch (SelectedLoaderType)
                {
                    case "Fabric":
                        ModLoaderInformations = (await MinecraftFabricInstaller.GetFabricBuildsFromMcVersionAsync(CoreManifestItem.Data.Id))
                            .Select(x =>
                            {
                                var data = new ModLoaderInformation()
                                {
                                    LoaderType = ModLoaderType.Fabric,
                                    McVersion = CoreManifestItem.Data.Id,
                                    Version = x.Loader.Version
                                }.CreateViewData<ModLoaderInformation, ModLoaderInformationViewData>();

                                data.Build = x;

                                return data;
                            }).ToList();
                        break;
                    case "Forge":
                        ModLoaderInformations = (await MinecraftForgeInstaller.GetForgeBuildsFromMcVersionAsync(CoreManifestItem.Data.Id))
                            .Select(x =>
                            {
                                var data = new ModLoaderInformation()
                                {
                                    LoaderType = ModLoaderType.Forge,
                                    McVersion = x.McVersion,
                                    Version = x.ForgeVersion,
                                    ReleaseTime = Environment.OSVersion.Version.Build < 22000 ? null : x.ModifiedTime
                                }.CreateViewData<ModLoaderInformation, ModLoaderInformationViewData>();

                                data.Build = x;

                                return data;
                            }).ToList();
                        break;
                    case "OptiFine":
                        ModLoaderInformations = (await MinecraftOptiFineInstaller.GetOptiFineBuildsFromMcVersionAsync(CoreManifestItem.Data.Id))
                            .Select(x =>
                             {
                                 var data = new ModLoaderInformation()
                                 {
                                     LoaderType = ModLoaderType.OptiFine,
                                     McVersion = x.McVersion,
                                     Version = $"{x.Type}_{x.Patch}"
                                 }.CreateViewData<ModLoaderInformation, ModLoaderInformationViewData>();

                                 data.Build = x;

                                 return data;
                             }).ToList();
                        break;
                }

                ModLoaderListLoading = Visibility.Collapsed;
            });
        }

        if (e.PropertyName != nameof(CanStart))
            CanStart = CoreManifestItem != null && (!EnableLoader || SelectedModLoader != null);

        if (e.PropertyName == nameof(ShowOldEdition) || e.PropertyName == nameof(ShowSnapshotEdition))
            CoreManifestItems = await GetCoreManifestItems();

        CanEnableLoaderTips = CanEnableLoader ? Visibility.Collapsed : Visibility.Visible;
        EnableLoaderExpander = CanEnableLoader && EnableLoader ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task<List<CoreManifestItemViewData>> GetCoreManifestItems()
    {
        if (CacheResources.CoreManifest == null)
            await CacheResources.BeginDownloadCoreManifest();

        return CacheResources.CoreManifest.Cores.Where(x =>
        {
            if (x.Type == "release")
                return true;

            if (ShowSnapshotEdition && x.Type == "snapshot")
                return true;

            if (ShowOldEdition && x.Type.StartsWith("old"))
                return true;

            return false;
        }).CreateCollectionViewData<CoreManifestItem, CoreManifestItemViewData>().ToList();
    }
}

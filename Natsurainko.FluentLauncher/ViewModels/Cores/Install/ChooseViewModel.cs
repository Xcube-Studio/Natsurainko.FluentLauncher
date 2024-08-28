using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Cores.Install;
using Nrk.FluentCore.Experimental.GameManagement.Installer.Data;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Install;

internal partial class ChooseViewModel : WizardViewModelBase
{
    public readonly VersionManifestItem _manifestItem;
    public readonly InstanceInstallConfig _installConfig;

    #region Properties
    public override bool CanNext => true;

    public override bool CanPrevious => false;

    [ObservableProperty]
    private List<ChooseModLoaderData> modLoaderDatas = [];

    #endregion

    public ChooseViewModel(VersionManifestItem manifestItem)
    {
        XamlPageType = typeof(ChoosePage);

        _manifestItem = manifestItem;
        _installConfig = new() { ManifestItem = manifestItem };

        modLoaderDatas = 
        [
            new(ModLoaderType.NeoForge, _manifestItem, modLoaderDatas),
            new(ModLoaderType.Forge, _manifestItem, modLoaderDatas),
            new(ModLoaderType.OptiFine, _manifestItem, modLoaderDatas),
            new(ModLoaderType.Fabric, _manifestItem, modLoaderDatas),
            new(ModLoaderType.Quilt, _manifestItem, modLoaderDatas),
        ];
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        var selected = ModLoaderDatas.Where(x => x.IsChecked).ToList();

        if (selected.Count != 0)
        {
            if (selected.Count == 1)
            {
                _installConfig.PrimaryLoader = selected[0];
            }
            else if (selected.Count == 2)
            {
                var primaryLoader = selected.Where(x => x.Type != ModLoaderType.OptiFine).First();
                selected.Remove(primaryLoader);

                _installConfig.PrimaryLoader = primaryLoader;
                _installConfig.SecondaryLoader = selected.First();
            }
        }

        return new ConfigViewModel(_installConfig);
    }
}


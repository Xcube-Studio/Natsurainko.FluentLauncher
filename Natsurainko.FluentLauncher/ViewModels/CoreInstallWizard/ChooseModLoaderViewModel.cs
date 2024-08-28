using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.CoreInstallWizard;
using Nrk.FluentCore.GameManagement.Installer;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;

internal partial class ChooseModLoaderViewModel : WizardViewModelBase
{
    public override bool CanNext => true;

    public override bool CanPrevious => false;

    public readonly VersionManifestItem _manifestItem;
    public readonly CoreInstallationInfo _coreInstallationInfo;

    [ObservableProperty]
    private List<ChooseModLoaderData> modLoaderDatas;

    public ChooseModLoaderViewModel(VersionManifestItem manifestItem)
    {
        XamlPageType = typeof(ChooseModLoaderPage);
        _manifestItem = manifestItem;

        _coreInstallationInfo = new() { ManifestItem = manifestItem };
        modLoaderDatas = new();

        modLoaderDatas.Add(new(ModLoaderType.NeoForge, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.Forge, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.OptiFine, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.Fabric, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.Quilt, _manifestItem, modLoaderDatas));
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        var selected = ModLoaderDatas.Where(x => x.IsChecked).ToList();

        if (selected.Any())
        {
            if (selected.Count == 1)
                _coreInstallationInfo.PrimaryLoader = selected[0];
            else if (selected.Count == 2)
            {
                var primaryLoader = selected.Where(x => x.Type != ModLoaderType.OptiFine).First();
                selected.Remove(primaryLoader);

                _coreInstallationInfo.PrimaryLoader = primaryLoader;
                _coreInstallationInfo.SecondaryLoader = selected.First();
            }
        }

        return new EnterCoreSettingsViewModel(_coreInstallationInfo);
    }
}

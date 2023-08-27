using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.CoreInstallWizard;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Enums;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;

internal partial class ChooseModLoaderViewModel : WizardViewModelBase
{
    public override bool CanNext => true;

    public override bool CanPrevious => false;

    private readonly VersionManifestItem _manifestItem;

    [ObservableProperty]
    private List<ChooseModLoaderData> modLoaderDatas;

    public ChooseModLoaderViewModel(VersionManifestItem manifestItem)
    {
        XamlPageType = typeof(ChooseModLoaderPage);
        _manifestItem = manifestItem;

        modLoaderDatas = new();

        modLoaderDatas.Add(new(ModLoaderType.NeoForge, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.Forge, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.OptiFine, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.Fabric, _manifestItem, modLoaderDatas));
        modLoaderDatas.Add(new(ModLoaderType.Quilt, _manifestItem, modLoaderDatas));
    }

    public override WizardViewModelBase GetNextViewModel() => new EnterCoreSettingsViewModel(_manifestItem);
}

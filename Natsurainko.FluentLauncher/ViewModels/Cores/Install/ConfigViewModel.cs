using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Cores.Install;
using Nrk.FluentCore.Experimental.GameManagement.Installer.Data;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Install;

internal partial class ConfigViewModel : WizardViewModelBase
{
    public readonly InstanceInstallConfig _installConfig;
    private readonly GameService _gameService = App.GetService<GameService>();

    public ConfigViewModel(InstanceInstallConfig instanceInstallConfig)
    {
        XamlPageType = typeof(ConfigPage);

        _installConfig = instanceInstallConfig;

        InstanceId = GetDefaultInstanceId();
        EnableIndependencyInstance = _installConfig.PrimaryLoader != null && _installConfig.SecondaryLoader != null;
    }

    #region Properties
    public override bool CanNext => CheckInstanceId();

    public override bool CanPrevious => true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public string instanceId;

    [ObservableProperty]
    public string nickName;

    [ObservableProperty]
    public bool enableIndependencyInstance;

    #endregion

    public override WizardViewModelBase GetNextViewModel()
    {
        _installConfig.NickName = NickName;
        _installConfig.InstanceId = InstanceId;
        _installConfig.EnableIndependencyInstance = EnableIndependencyInstance;

        return new OptionsViewModel(_installConfig);
    }

    string GetDefaultInstanceId()
    {
        if (_installConfig.PrimaryLoader == null)
            return _installConfig.ManifestItem.Id;

        List<string> tags = [_installConfig.ManifestItem.Id];

        if (_installConfig.PrimaryLoader != null)
            tags.Add(GetLoaderName(_installConfig.PrimaryLoader));

        if (_installConfig.SecondaryLoader != null)
            tags.Add(GetLoaderName(_installConfig.SecondaryLoader));

        return string.Join("-", tags);
    }

    bool CheckInstanceId()
    {
        if (string.IsNullOrEmpty(InstanceId) || _gameService.Games.Where(x => x.InstanceId.Equals(InstanceId)).Any())
            return false;

        return true;
    }

    static string GetLoaderName(ChooseModLoaderData modLoaderData)
    {
        ModLoaderType modLoaderType = modLoaderData.Type;

        switch (modLoaderType)
        {
            case ModLoaderType.NeoForge:
                var neoForgeInstallData = (ForgeInstallData)modLoaderData.SelectedInstallData;
                return $"NeoForge-{neoForgeInstallData.Version}";
            case ModLoaderType.Forge:
                var forgeInstallData = (ForgeInstallData)modLoaderData.SelectedInstallData;
                return $"Forge-{forgeInstallData.Version}";
            case ModLoaderType.Fabric:
                var fabricInstallData = (FabricInstallData)modLoaderData.SelectedInstallData;
                return $"Fabric-{fabricInstallData.Loader.Version}";
            case ModLoaderType.Quilt:
                var quiltInstallData = (QuiltInstallData)modLoaderData.SelectedInstallData;
                return $"Quilt-{quiltInstallData.Loader.Version}";
            case ModLoaderType.OptiFine:
                var optiFineInstallData = (OptiFineInstallData)modLoaderData.SelectedInstallData;
                return $"OptiFine-{optiFineInstallData.Type}_{optiFineInstallData.Patch}";
        }

        return null;
    }
}

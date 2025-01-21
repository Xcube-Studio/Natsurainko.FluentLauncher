using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.Models.UI;

public partial class ChooseModLoaderData : ObservableObject
{
    private readonly VersionManifestItem _manifestItem;
    private readonly List<ChooseModLoaderData> _chooseModLoaders;

    private readonly CacheInterfaceService cacheInterfaceService = App.GetService<CacheInterfaceService>();

    public ChooseModLoaderData(ModLoaderType type, VersionManifestItem manifestItem, List<ChooseModLoaderData> chooseModLoaders)
    {
        Type = type;
        Description = ResourceUtils.GetValue("CoreInstallWizard", "ChooseModLoaderPage", $"_{Type}");

        _manifestItem = manifestItem;
        _chooseModLoaders = chooseModLoaders;

        Task.Run(LoadInstallDatas);
    }

    #region Properties

    [ObservableProperty]
    public partial ModLoaderType Type { get; set; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnable))]
    public partial bool IsSupported { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnable))]
    public partial bool IsCompatible { get; set; } = true;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial string DisplayText { get; set; } = LocalizedStrings.CoreInstallWizard_ChooseModLoaderPage__Loading;

    [ObservableProperty]
    public partial object[] InstallDatas { get; set; }

    [ObservableProperty]
    public partial object SelectedInstallData { get; set; }

    public bool IsEnable => IsSupported && IsCompatible;

    #endregion

    async Task LoadInstallDatas()
    {
        object[] installDatas = [];
        string jsonContent;

        try
        {
            string requestUrl = Type switch
            {
                ModLoaderType.NeoForge => $"https://bmclapi2.bangbang93.com/neoforge/list/{_manifestItem.Id}",
                ModLoaderType.Forge => $"https://bmclapi2.bangbang93.com/forge/minecraft/{_manifestItem.Id}",
                ModLoaderType.OptiFine => $"https://bmclapi2.bangbang93.com/optifine/{_manifestItem.Id}",
                ModLoaderType.Fabric => $"https://meta.fabricmc.net/v2/versions/loader/{_manifestItem.Id}",
                ModLoaderType.Quilt => $"https://meta.quiltmc.org/v3/versions/loader/{_manifestItem.Id}",
                _ => throw new NotImplementedException()
            };

            jsonContent = await cacheInterfaceService.RequestStringAsync(requestUrl, Services.Network.Data.InterfaceRequestMethod.AlwaysLatest);

            installDatas = Type switch
            {
                ModLoaderType.NeoForge => [.. JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.ForgeInstallDataArray).OrderByDescending(x => x, new ForgeVersionComparer())],
                ModLoaderType.Forge => [.. JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.ForgeInstallDataArray).OrderByDescending(x => x, new ForgeVersionComparer())],
                ModLoaderType.OptiFine => [.. JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.OptiFineInstallDataArray).OrderByDescending(x => x, new OptiFineVersionComparer())],
                ModLoaderType.Fabric => JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.FabricInstallDataArray),
                ModLoaderType.Quilt => JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.QuiltInstallDataArray),
                _ => throw new InvalidOperationException()
            };
        }
        catch (Exception ex)
        {

        }

        App.DispatcherQueue.TryEnqueue(() =>
        {
            IsLoading = false;

            if (installDatas.Length != 0)
            {
                InstallDatas = installDatas;
                SelectedInstallData = InstallDatas.FirstOrDefault();
                IsSupported = true;
            }
            else DisplayText = LocalizedStrings.CoreInstallWizard_ChooseModLoaderPage__NotSupported;
        });
    }

    public void HandleCompatible()
    {
        bool compatible = true;
        var uncompatibleModLoaderTypes = new List<ModLoaderType>();

        switch (Type)
        {
            case ModLoaderType.Forge:
                uncompatibleModLoaderTypes.Add(ModLoaderType.NeoForge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Fabric);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
            case ModLoaderType.Fabric:
                uncompatibleModLoaderTypes.Add(ModLoaderType.NeoForge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Forge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
            case ModLoaderType.Quilt:
                uncompatibleModLoaderTypes.Add(ModLoaderType.NeoForge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Forge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Fabric);
                uncompatibleModLoaderTypes.Add(ModLoaderType.OptiFine);
                break;
            case ModLoaderType.NeoForge:
                uncompatibleModLoaderTypes.Add(ModLoaderType.Forge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Fabric);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
            case ModLoaderType.OptiFine:
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
        }

        foreach (var data in _chooseModLoaders.Where(x => x.IsChecked))
            if (uncompatibleModLoaderTypes.Contains(data.Type))
            {
                compatible = false;
                break;
            }

        IsCompatible = compatible;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsChecked))
            _chooseModLoaders.ForEach(x => x.HandleCompatible());
    }
}

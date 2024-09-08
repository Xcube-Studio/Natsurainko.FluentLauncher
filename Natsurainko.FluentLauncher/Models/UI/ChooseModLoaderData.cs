﻿using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private ModLoaderType type;

    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnable))]
    private bool isSupported = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnable))]
    private bool isCompatible = true;

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private string displayText = ResourceUtils.GetValue("CoreInstallWizard", "ChooseModLoaderPage", "_Loading");

    [ObservableProperty]
    private object[] installDatas;

    [ObservableProperty]
    private object selectedInstallData;

    public bool IsEnable => IsSupported && IsCompatible;

    #endregion

    async Task LoadInstallDatas()
    {
        object[] installDatas = [];

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

            string jsonContent = await cacheInterfaceService.RequestStringAsync(requestUrl, Services.Network.Data.InterfaceRequestMethod.AlwaysLatest);

            installDatas = Type switch
            {
                ModLoaderType.NeoForge => JsonNode.Parse(jsonContent).Deserialize<ForgeInstallData[]>()!,
                ModLoaderType.Forge => JsonNode.Parse(jsonContent).Deserialize<ForgeInstallData[]>()!,
                ModLoaderType.OptiFine => JsonNode.Parse(jsonContent).Deserialize<OptiFineInstallData[]>()!,
                ModLoaderType.Fabric => JsonNode.Parse(jsonContent).Deserialize<FabricInstallData[]>()!,
                ModLoaderType.Quilt => JsonNode.Parse(jsonContent).Deserialize<QuiltInstallData[]>()!,
                _ => throw new InvalidOperationException()
            };
        }
        catch (Exception)
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
            else DisplayText = ResourceUtils.GetValue("CoreInstallWizard", "ChooseModLoaderPage", "_NotSupported");
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
